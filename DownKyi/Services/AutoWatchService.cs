using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DownKyi.Core.BiliApi.Favorites;
using DownKyi.Core.BiliApi.Favorites.Models;
using DownKyi.Core.BiliApi.VideoStream;
using DownKyi.Core.Logging;
using DownKyi.Core.Settings;
using DownKyi.Core.Storage;
using DownKyi.Events;
using DownKyi.Services.Download;
using DownKyi.Utils;
using Prism.Events;
using Console = DownKyi.Core.Utils.Debugging.Console;
using IDialogService = DownKyi.PrismExtension.Dialog.IDialogService;

namespace DownKyi.Services;

/// <summary>
/// 收藏夹自动监控下载服务
/// 定时轮询指定收藏夹，发现新视频后自动加入下载队列
/// </summary>
public class AutoWatchService : IDisposable
{
    private const string Tag = "AutoWatchService";

    private readonly DownloadStorageService _downloadStorageService;
    private readonly IEventAggregator _eventAggregator;
    private readonly IDialogService? _dialogService;

    private PeriodicTimer? _timer;
    private CancellationTokenSource? _cts;
    private Task? _loopTask;

    /// <summary>
    /// 内存缓存：已处理的 avid 集合（mediaId → avid 映射）
    /// </summary>
    private readonly ConcurrentDictionary<long, byte> _knownAvids = new();

    private long _mediaId;
    private int _intervalMinutes;
    private bool _disposed;

    public AutoWatchService(long mediaId, int intervalMinutes,
        DownloadStorageService downloadStorageService,
        IEventAggregator eventAggregator,
        IDialogService? dialogService)
    {
        _mediaId = mediaId;
        _intervalMinutes = intervalMinutes;
        _downloadStorageService = downloadStorageService;
        _eventAggregator = eventAggregator;
        _dialogService = dialogService;
    }

    /// <summary>
    /// 启动定时监控
    /// </summary>
    public void Start()
    {
        if (_timer != null)
        {
            LogManager.Info(Tag,"AutoWatchService 已经在运行中，忽略重复启动");
            return;
        }

        try
        {
            // 从 SQLite 加载已处理的 avid
            var processedAvids = _downloadStorageService.GetProcessedAvids(_mediaId);
            foreach (var avid in processedAvids)
            {
                _knownAvids.TryAdd(avid, 0);
            }

            LogManager.Info(Tag, $"自动监控已启动：mediaId={_mediaId}, interval={_intervalMinutes}min, 已处理记录={_knownAvids.Count}");

            _cts = new CancellationTokenSource();
            _timer = new PeriodicTimer(TimeSpan.FromMinutes(_intervalMinutes));
            _loopTask = Task.Run(() => RunLoopAsync(_cts.Token));
        }
        catch (Exception e)
        {
            LogManager.Error(Tag, e);
            Console.PrintLine("AutoWatchService.Start()发生异常: {0}", e);
        }
    }

    /// <summary>
    /// 停止定时监控
    /// </summary>
    public async Task StopAsync()
    {
        if (_timer == null)
        {
            return;
        }

        try
        {
            _cts?.Cancel();
            _timer?.Dispose();
            _timer = null;

            if (_loopTask != null)
            {
                await _loopTask;
                _loopTask = null;
            }

            LogManager.Info(Tag, "自动监控已停止");
        }
        catch (OperationCanceledException) { /* 预期的取消异常 */ }
        catch (Exception e)
        {
            LogManager.Error(Tag, e);
            Console.PrintLine("AutoWatchService.StopAsync()发生异常: {0}", e);
        }
    }

    /// <summary>
    /// 同步停止（用于应用退出时）
    /// </summary>
    public void Stop()
    {
        _cts?.Cancel();
        _timer?.Dispose();
        _timer = null;

        try
        {
            // 等待最多 10 秒让当前任务完成
            _loopTask?.Wait(TimeSpan.FromSeconds(10));
        }
        catch (AggregateException ae) when (ae.InnerException is OperationCanceledException)
        {
            /* 预期的取消异常 */
        }
        catch (Exception e)
        {
            LogManager.Error(Tag, e);
            Console.PrintLine("AutoWatchService.Stop()发生异常: {0}", e);
        }
    }

    /// <summary>
    /// 定时器主循环（支持热更新：每次轮询前检测间隔变化）
    /// </summary>
    private async Task RunLoopAsync(CancellationToken ct)
    {
        try
        {
            // 启动时立即执行一次检查
            await DoCheck();

            while (!ct.IsCancellationRequested)
            {
                var settings = SettingsManager.GetInstance();

                // 检查收藏夹ID是否被修改
                var currentMediaId = settings.GetAutoWatchMediaId();
                if (currentMediaId != _mediaId && currentMediaId > 0)
                {
                    _mediaId = currentMediaId;
                    _knownAvids.Clear();
                    var avids = _downloadStorageService.GetProcessedAvids(_mediaId);
                    foreach (var avid in avids) { _knownAvids.TryAdd(avid, 0); }
                    LogManager.Info(Tag, $"监控收藏夹已热更新为 {_mediaId}，已处理记录={_knownAvids.Count}");
                }

                // 检查轮询间隔是否被修改
                var currentInterval = settings.GetAutoWatchIntervalMinutes();
                if (currentInterval != _intervalMinutes)
                {
                    _intervalMinutes = currentInterval;
                    _timer?.Dispose();
                    _timer = new PeriodicTimer(TimeSpan.FromMinutes(_intervalMinutes));
                    LogManager.Info(Tag, $"轮询间隔已热更新为 {_intervalMinutes} 分钟");
                }

                await _timer!.WaitForNextTickAsync(ct);
                await DoCheck();
            }
        }
        catch (OperationCanceledException)
        {
            /* 预期的取消异常 */
        }
    }

    /// <summary>
    /// 执行一次检查和下载
    /// </summary>
    private async Task DoCheck()
    {
        try
        {
            var newCount = await CheckAndDownloadAsync();
            if (newCount > 0)
            {
                LogManager.Info(Tag, $"自动监控：发现 {newCount} 个新视频并加入下载队列");
                _eventAggregator.GetEvent<MessageEvent>()
                    .Publish($"自动监控：发现 {newCount} 个新视频，已加入下载队列");
            }
        }
        catch (OperationCanceledException)
        {
            throw; // 穿透给上层处理
        }
        catch (Exception ex)
        {
            LogManager.Error(Tag, ex);
            Console.PrintLine("RunLoopAsync()执行异常: {0}", ex);
        }
    }

    /// <summary>
    /// 检查收藏夹并下载新视频，返回新增视频数量
    /// </summary>
    private async Task<int> CheckAndDownloadAsync()
    {
        // 第一步：使用 GetFavoritesMediaId 获取收藏夹全部ID（单次请求，含 bvid）
        var mediaIds = FavoritesResource.GetFavoritesMediaId(_mediaId);
        if (mediaIds == null || mediaIds.Count == 0)
        {
            LogManager.Debug(Tag, $"收藏夹 {_mediaId} 没有内容或获取失败");
            return 0;
        }

        // 第二步：增量检测
        var newMediaIds = mediaIds
            .Where(m => m.Type == 2 && !_knownAvids.ContainsKey(m.Id)) // Type=2 是视频
            .ToList();

        if (newMediaIds.Count == 0)
        {
            return 0;
        }

        LogManager.Debug(Tag, $"检测到 {newMediaIds.Count} 个新视频");

        // 第三步：获取下载目录
        var settings = SettingsManager.GetInstance();
        var saveDirectory = settings.GetAutoWatchSaveDirectory();
        if (string.IsNullOrEmpty(saveDirectory))
        {
            // 回退到全局默认下载目录
            saveDirectory = settings.GetSaveVideoRootPath();
        }

        if (string.IsNullOrEmpty(saveDirectory))
        {
            LogManager.Info(Tag,"自动监控：未配置下载目录，跳过下载");
            _eventAggregator.GetEvent<MessageEvent>()
                .Publish("自动监控：未配置下载目录，请先在设置中配置");
            return 0;
        }

        // 确保目录存在
        if (!System.IO.Directory.Exists(saveDirectory))
        {
            System.IO.Directory.CreateDirectory(saveDirectory);
        }

        // 获取下载内容设置
        var (downloadAudio, downloadVideo, downloadDanmaku, downloadSubtitle, downloadCover) =
            settings.GetAutoWatchDownloadContent();

        // 第四步：逐个加入下载队列
        var successCount = 0;
        var records = new List<(long Avid, string Bvid, string? Title)>();

        foreach (var mediaId in newMediaIds)
        {
            try
            {
                var addToDownloadService = new AddToDownloadService(mediaId.Bvid, PlayStreamType.Video);

                // 设置下载内容（静默模式）
                addToDownloadService.SetDownloadContentAndDirectory(
                    downloadAudio, downloadVideo, downloadDanmaku, downloadSubtitle, downloadCover);

                // 获取视频信息并解析
                addToDownloadService.GetVideo();
                addToDownloadService.ParseVideo(new VideoInfoService(mediaId.Bvid));

                // 加入下载队列
                var count = await addToDownloadService.AddToDownload(
                    _eventAggregator, _dialogService, saveDirectory, isAll: true);

                if (count > 0)
                {
                    successCount += count;
                }

                // 记录到内存和数据库
                _knownAvids.TryAdd(mediaId.Id, 0);
                records.Add((mediaId.Id, mediaId.Bvid, null));
            }
            catch (Exception ex)
            {
                // 单个视频下载失败不影响后续处理
                LogManager.Error(Tag, $"自动下载视频失败: avid={mediaId.Id}, bvid={mediaId.Bvid}");
                LogManager.Error(Tag, ex);
                Console.PrintLine("自动下载视频失败: avid={0}, bvid={1}, ex={2}", mediaId.Id, mediaId.Bvid, ex);
            }
        }

        // 批量持久化已处理记录
        if (records.Count > 0)
        {
            _downloadStorageService.InsertAutoWatchRecordBatch(_mediaId, records);
        }

        return successCount;
    }

    /// <summary>
    /// 更新监控参数（由设置变更触发）
    /// </summary>
    public void UpdateSettings(long mediaId, int intervalMinutes)
    {
        var needRestart = _mediaId != mediaId || _intervalMinutes != intervalMinutes;
        _mediaId = mediaId;
        _intervalMinutes = intervalMinutes;

        if (needRestart && _timer != null)
        {
            LogManager.Info(Tag, $"自动监控参数变更，重启服务：mediaId={mediaId}, interval={intervalMinutes}min");
            Stop();
            Start();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _cts?.Cancel();
        _cts?.Dispose();
        _timer?.Dispose();
        _loopTask?.Dispose();
    }
}
