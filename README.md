# 哔哩下载姬(跨平台版)

<div align="center">

[![GitHub Repo stars](https://img.shields.io/github/stars/Fr33Gn3M/downkyi)](https://github.com/Fr33Gn3M/downkyi/stargazers)
[![GitHub forks](https://img.shields.io/github/forks/Fr33Gn3M/downkyi)](https://github.com/Fr33Gn3M/downkyi/network)
[![GitHub issues](https://img.shields.io/github/issues/Fr33Gn3M/downkyi)](https://github.com/Fr33Gn3M/downkyi/issues)
[![LICENSE](https://img.shields.io/github/license/Fr33Gn3M/downkyi)](https://github.com/Fr33Gn3M/downkyi/blob/main/LICENSE)

</div>

## 下载

[![GitHub release (latest by date)](https://img.shields.io/github/v/release/Fr33Gn3M/downkyi)](https://github.com/Fr33Gn3M/downkyi/releases/latest)
[![GitHub Release Date](https://img.shields.io/github/release-date/Fr33Gn3M/downkyi)](https://github.com/Fr33Gn3M/downkyi/releases/latest)
[![GitHub all releases](https://img.shields.io/github/downloads/Fr33Gn3M/downkyi/total)](https://github.com/Fr33Gn3M/downkyi/releases/latest)

[更新日志](CHANGELOG.md)

## 介绍

本项目 Fork 自 [yaobiao131/downkyicore](https://github.com/yaobiao131/downkyicore)，后者基于 [leiurayer/downkyi](https://github.com/leiurayer/downkyi) 使用 [AvaloniaUI](https://github.com/AvaloniaUI/Avalonia) 重新开发的跨平台版本，支持 Windows、Linux、macOS。

### 本 Fork 新增功能

- **收藏夹自动监控下载**：定时轮询指定收藏夹，发现新视频后自动解析并加入下载队列，支持热更新设置
- 项目框架升级至 .NET 8.0

## 使用说明

- 软件自带 .NET 8、ffmpeg、aria2 运行环境，无需自行安装
- 默认下载路径:
  - Windows: 软件运行目录下的 Media 文件夹
  - macOS: ~/Library/Application Support/DownKyi/Media
  - Linux: ~/.config/DownKyi/Media

## 免责申明

1. 本软件只提供视频解析，不提供任何资源上传、存储到服务器的功能。
2. 本软件仅解析来自B站的内容，不会对解析到的音视频进行二次编码，部分视频会进行有限的格式转换、拼接等操作。
3. 本软件解析得到的所有内容均来自B站UP主上传、分享，其版权均归原作者所有。内容提供者、上传者(UP主)应对其提供、上传的内容承担全部责任。
4. **本软件提供的所有内容，仅可用作学习交流使用，未经原作者授权，禁止用于其他用途。请在下载24小时内删除。为尊重作者版权，请前往资源的原始发布网站观看，支持原创，谢谢。**
5. 因使用本软件产生的版权问题，软件作者概不负责。
