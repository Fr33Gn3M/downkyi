using System.Collections.Generic;
using System.Linq;
using DownKyi.Core.Settings;
using DownKyi.Events;
using DownKyi.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace DownKyi.ViewModels.Settings;

public class ViewAutoWatchViewModel : ViewModelBase
{
    public const string Tag = "PageSettingsAutoWatch";

    private bool _isOnNavigatedTo;

    #region 页面属性申明

    private bool _autoWatchEnabled;

    public bool AutoWatchEnabled
    {
        get => _autoWatchEnabled;
        set => SetProperty(ref _autoWatchEnabled, value);
    }

    private string? _mediaIdText;

    public string? MediaIdText
    {
        get => _mediaIdText;
        set => SetProperty(ref _mediaIdText, value);
    }

    private List<IntervalDisplay> _intervals;

    public List<IntervalDisplay> Intervals
    {
        get => _intervals;
        set => SetProperty(ref _intervals, value);
    }

    private IntervalDisplay? _selectedInterval;

    public IntervalDisplay? SelectedInterval
    {
        get => _selectedInterval;
        set => SetProperty(ref _selectedInterval, value);
    }

    private string? _saveDirectory;

    public string? SaveDirectory
    {
        get => _saveDirectory;
        set => SetProperty(ref _saveDirectory, value);
    }

    private bool _downloadAudio;

    public bool DownloadAudio
    {
        get => _downloadAudio;
        set => SetProperty(ref _downloadAudio, value);
    }

    private bool _downloadVideo;

    public bool DownloadVideo
    {
        get => _downloadVideo;
        set => SetProperty(ref _downloadVideo, value);
    }

    private bool _downloadDanmaku;

    public bool DownloadDanmaku
    {
        get => _downloadDanmaku;
        set => SetProperty(ref _downloadDanmaku, value);
    }

    private bool _downloadSubtitle;

    public bool DownloadSubtitle
    {
        get => _downloadSubtitle;
        set => SetProperty(ref _downloadSubtitle, value);
    }

    private bool _downloadCover;

    public bool DownloadCover
    {
        get => _downloadCover;
        set => SetProperty(ref _downloadCover, value);
    }

    #endregion

    public ViewAutoWatchViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
    {
        #region 属性初始化

        Intervals = new List<IntervalDisplay>
        {
            new() { Name = "10 分钟", Minutes = 10 },
            new() { Name = "30 分钟", Minutes = 30 },
            new() { Name = "60 分钟", Minutes = 60 }
        };

        #endregion
    }

    /// <summary>
    /// 导航到页面时执行
    /// </summary>
    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        base.OnNavigatedTo(navigationContext);

        _isOnNavigatedTo = true;

        var settings = SettingsManager.GetInstance();

        // 启用状态
        AutoWatchEnabled = settings.GetIsAutoWatchEnabled();

        // 收藏夹ID
        var mediaId = settings.GetAutoWatchMediaId();
        MediaIdText = mediaId > 0 ? mediaId.ToString() : "";

        // 轮询间隔
        var intervalMinutes = settings.GetAutoWatchIntervalMinutes();
        SelectedInterval = Intervals.FirstOrDefault(t => t.Minutes == intervalMinutes);

        // 保存目录
        var saveDir = settings.GetAutoWatchSaveDirectory();
        SaveDirectory = string.IsNullOrEmpty(saveDir) ? "" : saveDir;

        // 下载内容
        var (downloadAudio, downloadVideo, downloadDanmaku, downloadSubtitle, downloadCover) =
            settings.GetAutoWatchDownloadContent();
        DownloadAudio = downloadAudio;
        DownloadVideo = downloadVideo;
        DownloadDanmaku = downloadDanmaku;
        DownloadSubtitle = downloadSubtitle;
        DownloadCover = downloadCover;

        _isOnNavigatedTo = false;
    }

    #region 命令申明

    // 启用/禁用自动监控
    private DelegateCommand? _autoWatchEnabledCommand;

    public DelegateCommand AutoWatchEnabledCommand =>
        _autoWatchEnabledCommand ??= new DelegateCommand(ExecuteAutoWatchEnabledCommand);

    private void ExecuteAutoWatchEnabledCommand()
    {
        var isSucceed = SettingsManager.GetInstance().SetIsAutoWatchEnabled(AutoWatchEnabled);
        PublishTip(isSucceed);
        App.RefreshAutoWatch();
    }

    // 收藏夹ID变更
    private DelegateCommand? _mediaIdCommand;

    public DelegateCommand MediaIdCommand =>
        _mediaIdCommand ??= new DelegateCommand(ExecuteMediaIdCommand);

    private void ExecuteMediaIdCommand()
    {
        if (long.TryParse(MediaIdText, out var mediaId))
        {
            var isSucceed = SettingsManager.GetInstance().SetAutoWatchMediaId(mediaId);
            PublishTip(isSucceed);
        }
    }

    // 轮询间隔变更
    private DelegateCommand? _intervalCommand;

    public DelegateCommand IntervalCommand =>
        _intervalCommand ??= new DelegateCommand(ExecuteIntervalCommand);

    private void ExecuteIntervalCommand()
    {
        if (SelectedInterval != null)
        {
            var isSucceed = SettingsManager.GetInstance().SetAutoWatchIntervalMinutes(SelectedInterval.Minutes);
            PublishTip(isSucceed);
        }
    }

    // 保存目录变更
    private DelegateCommand? _saveDirectoryCommand;

    public DelegateCommand SaveDirectoryCommand =>
        _saveDirectoryCommand ??= new DelegateCommand(ExecuteSaveDirectoryCommand);

    private void ExecuteSaveDirectoryCommand()
    {
        var isSucceed = SettingsManager.GetInstance().SetAutoWatchSaveDirectory(SaveDirectory ?? "");
        PublishTip(isSucceed);
    }

    // 下载内容变更（统一保存）
    private DelegateCommand? _downloadContentCommand;

    public DelegateCommand DownloadContentCommand =>
        _downloadContentCommand ??= new DelegateCommand(ExecuteDownloadContentCommand);

    private void ExecuteDownloadContentCommand()
    {
        var isSucceed = SettingsManager.GetInstance().SetAutoWatchDownloadContent(
            DownloadAudio, DownloadVideo, DownloadDanmaku, DownloadSubtitle, DownloadCover);
        PublishTip(isSucceed);
    }

    #endregion

    /// <summary>
    /// 发布提示消息
    /// </summary>
    private void PublishTip(bool isSucceed)
    {
        if (_isOnNavigatedTo)
        {
            return;
        }

        EventAggregator.GetEvent<MessageEvent>().Publish(
            isSucceed
                ? DictionaryResource.GetString("TipSettingUpdated")
                : DictionaryResource.GetString("TipSettingFailed"));
    }
}

/// <summary>
/// 轮询间隔显示模型
/// </summary>
public class IntervalDisplay
{
    public string Name { get; set; } = "";
    public int Minutes { get; set; }
}
