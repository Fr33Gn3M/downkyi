namespace DownKyi.Core.Settings.Models;

/// <summary>
/// 自动监控收藏夹设置
/// </summary>
public class AutoWatchSettings
{
    /// <summary>
    /// 是否启用自动监控
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// 监控的收藏夹ID
    /// </summary>
    public long MediaId { get; set; } = 0;

    /// <summary>
    /// 轮询间隔（分钟），默认10分钟
    /// </summary>
    public int IntervalMinutes { get; set; } = 10;

    /// <summary>
    /// 下载保存目录，为空则使用全局默认
    /// </summary>
    public string SaveDirectory { get; set; } = "";

    /// <summary>
    /// 下载音频
    /// </summary>
    public bool DownloadAudio { get; set; } = true;

    /// <summary>
    /// 下载视频
    /// </summary>
    public bool DownloadVideo { get; set; } = true;

    /// <summary>
    /// 下载弹幕
    /// </summary>
    public bool DownloadDanmaku { get; set; } = true;

    /// <summary>
    /// 下载字幕
    /// </summary>
    public bool DownloadSubtitle { get; set; } = true;

    /// <summary>
    /// 下载封面
    /// </summary>
    public bool DownloadCover { get; set; } = true;
}
