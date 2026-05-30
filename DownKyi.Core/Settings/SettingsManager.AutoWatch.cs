namespace DownKyi.Core.Settings;

public partial class SettingsManager
{
    // 默认启用状态
    private const bool AutoWatchEnabled = false;

    // 默认轮询间隔（分钟）
    private const int AutoWatchIntervalMinutes = 10;

    // 默认下载内容
    private const bool AutoWatchDownloadAudio = true;
    private const bool AutoWatchDownloadVideo = true;
    private const bool AutoWatchDownloadDanmaku = true;
    private const bool AutoWatchDownloadSubtitle = true;
    private const bool AutoWatchDownloadCover = true;

    /// <summary>
    /// 获取自动监控启用状态
    /// </summary>
    public bool GetIsAutoWatchEnabled()
    {
        if (_appSettings.AutoWatch.Enabled == false && _appSettings.AutoWatch.MediaId == 0)
        {
            // 第一次获取，先设置默认值
            SetIsAutoWatchEnabled(AutoWatchEnabled);
            return AutoWatchEnabled;
        }

        return _appSettings.AutoWatch.Enabled;
    }

    /// <summary>
    /// 设置自动监控启用状态
    /// </summary>
    public bool SetIsAutoWatchEnabled(bool enabled)
    {
        return SetProperty(
            _appSettings.AutoWatch.Enabled,
            enabled,
            v => _appSettings.AutoWatch.Enabled = v);
    }

    /// <summary>
    /// 获取监控的收藏夹ID
    /// </summary>
    public long GetAutoWatchMediaId()
    {
        return _appSettings.AutoWatch.MediaId;
    }

    /// <summary>
    /// 设置监控的收藏夹ID
    /// </summary>
    public bool SetAutoWatchMediaId(long mediaId)
    {
        return SetProperty(
            _appSettings.AutoWatch.MediaId,
            mediaId,
            v => _appSettings.AutoWatch.MediaId = v);
    }

    /// <summary>
    /// 获取轮询间隔（分钟）
    /// </summary>
    public int GetAutoWatchIntervalMinutes()
    {
        if (_appSettings.AutoWatch.IntervalMinutes <= 0)
        {
            SetAutoWatchIntervalMinutes(AutoWatchIntervalMinutes);
            return AutoWatchIntervalMinutes;
        }

        return _appSettings.AutoWatch.IntervalMinutes;
    }

    /// <summary>
    /// 设置轮询间隔（分钟）
    /// </summary>
    public bool SetAutoWatchIntervalMinutes(int minutes)
    {
        return SetProperty(
            _appSettings.AutoWatch.IntervalMinutes,
            minutes,
            v => _appSettings.AutoWatch.IntervalMinutes = v);
    }

    /// <summary>
    /// 获取自动监控的下载保存目录
    /// </summary>
    public string GetAutoWatchSaveDirectory()
    {
        return _appSettings.AutoWatch.SaveDirectory;
    }

    /// <summary>
    /// 设置自动监控的下载保存目录
    /// </summary>
    public bool SetAutoWatchSaveDirectory(string directory)
    {
        return SetProperty(
            _appSettings.AutoWatch.SaveDirectory,
            directory,
            v => _appSettings.AutoWatch.SaveDirectory = v);
    }

    /// <summary>
    /// 获取自动监控的下载内容设置
    /// </summary>
    public (bool DownloadAudio, bool DownloadVideo, bool DownloadDanmaku, bool DownloadSubtitle, bool DownloadCover) GetAutoWatchDownloadContent()
    {
        return (
            _appSettings.AutoWatch.DownloadAudio,
            _appSettings.AutoWatch.DownloadVideo,
            _appSettings.AutoWatch.DownloadDanmaku,
            _appSettings.AutoWatch.DownloadSubtitle,
            _appSettings.AutoWatch.DownloadCover
        );
    }

    /// <summary>
    /// 设置自动监控的下载内容
    /// </summary>
    public bool SetAutoWatchDownloadContent(bool downloadAudio, bool downloadVideo, bool downloadDanmaku, bool downloadSubtitle, bool downloadCover)
    {
        SetProperty(_appSettings.AutoWatch.DownloadAudio, downloadAudio, v => _appSettings.AutoWatch.DownloadAudio = v);
        SetProperty(_appSettings.AutoWatch.DownloadVideo, downloadVideo, v => _appSettings.AutoWatch.DownloadVideo = v);
        SetProperty(_appSettings.AutoWatch.DownloadDanmaku, downloadDanmaku, v => _appSettings.AutoWatch.DownloadDanmaku = v);
        SetProperty(_appSettings.AutoWatch.DownloadSubtitle, downloadSubtitle, v => _appSettings.AutoWatch.DownloadSubtitle = v);
        SetProperty(_appSettings.AutoWatch.DownloadCover, downloadCover, v => _appSettings.AutoWatch.DownloadCover = v);
        return true;
    }
}
