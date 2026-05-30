namespace DownKyi.Models;

/// <summary>
/// 自动监控已处理视频记录
/// </summary>
public class AutoWatchRecord
{
    public long Id { get; set; }
    public long MediaId { get; set; }
    public long Avid { get; set; }
    public string Bvid { get; set; } = "";
    public string? Title { get; set; }
    public string CreateTime { get; set; } = "";
}
