using System;
using System.IO;
using System.Text.RegularExpressions;

namespace DownKyi.Models;

public class AppInfo
{
    public string Name { get; } = "哔哩下载姬";
    public int VersionCode { get; }
    public string VersionName { get; }

    public AppInfo()
    {
        // 从 version.txt 读取版本号（单一来源）
        var version = "1.0.0"; // 默认值
        try
        {
            var versionFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "version.txt");
            if (File.Exists(versionFile))
            {
                var text = File.ReadAllText(versionFile).Trim();
                if (!string.IsNullOrEmpty(text) && Regex.IsMatch(text, @"^\d+\.\d+\.\d+"))
                {
                    version = text;
                }
            }
        }
        catch { /* 读取失败使用默认值 */ }

        var parts = version.Split('.');
        var a = int.Parse(parts[0]);
        var b = int.Parse(parts[1]);
        var c = int.Parse(parts[2]);

        VersionCode = a * 10000 + b * 100 + c;

#if DEBUG
        VersionName = $"{a}.{b}.{c}-debug";
#else
        VersionName = $"{a}.{b}.{c}";
#endif
    }

    public static int VersionNameToCode(string versionName)
    {
        var code = 0;

        var isMatch = Regex.IsMatch(versionName, @"^v?([1-9]\d|\d).([1-9]\d|\d).([1-9]\d|\d)$");
        if (!isMatch)
        {
            return 0;
        }

        var pattern = @"([1-9]\d|\d)";
        var m = Regex.Matches(versionName, pattern);
        if (m.Count == 3)
        {
            var i = 2;
            foreach (var item in m)
            {
                code += int.Parse(item.ToString()!) * (int)Math.Pow(100, i);
                i--;
            }
        }

        return code;
    }
}