// Zen Media Saver - Reklamsız Medya ve Müzik İndirici
// Geliştirici: Serkan ŞENTÜRK
// Telif Hakkı © 2026 Serkan ŞENTÜRK. Tüm Hakları Saklıdır.

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace ZenMediaSaver
{
    public enum DownloadFormat
    {
        BestVideo,   // 4K / 1080p MP4
        Standard720, // 720p MP4
        AudioMp3     // High Quality MP3
    }

    public class DownloadProgressArgs
    {
        public double Percentage { get; set; }
        public string Speed { get; set; }
        public string Eta { get; set; }
        public string TotalSize { get; set; }
        public string StatusText { get; set; }
    }

    public static class DownloaderEngine
    {
        private const string YtDlpUrl = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe";
        private static readonly string EngineDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ZenMediaSaver", "Engine");
        public static readonly string YtDlpPath = Path.Combine(EngineDir, "yt-dlp.exe");

        public static bool IsEngineReady()
        {
            // Check local dir first
            string localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yt-dlp.exe");
            if (File.Exists(localPath)) return true;

            return File.Exists(YtDlpPath);
        }

        public static string GetActiveEnginePath()
        {
            string localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yt-dlp.exe");
            if (File.Exists(localPath)) return localPath;
            return YtDlpPath;
        }

        public static void EnsureEngineAsync(Action<double> onProgress, Action<bool, string> onComplete)
        {
            if (IsEngineReady())
            {
                onComplete(true, null);
                return;
            }

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    if (!Directory.Exists(EngineDir))
                    {
                        Directory.CreateDirectory(EngineDir);
                    }

                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; // TLS 1.2

                    using (WebClient client = new WebClient())
                    {
                        client.Headers.Add("User-Agent", "ZenMediaSaver-Bootstrap");
                        client.DownloadProgressChanged += (s, e) =>
                        {
                            onProgress(e.ProgressPercentage);
                        };

                        client.DownloadFileCompleted += (s, e) =>
                        {
                            if (e.Error != null)
                            {
                                onComplete(false, "İndirme motoru sunucusuna bağlanılamadı: " + e.Error.Message);
                            }
                            else
                            {
                                onComplete(true, null);
                            }
                        };

                        client.DownloadFileAsync(new Uri(YtDlpUrl), YtDlpPath);
                    }
                }
                catch (Exception ex)
                {
                    onComplete(false, ex.Message);
                }
            });
        }

        public static Process StartDownload(
            string url,
            DownloadFormat format,
            string outputDirectory,
            Action<DownloadProgressArgs> onProgress,
            Action<bool, string, string> onFinished)
        {
            string engineExe = GetActiveEnginePath();
            if (!File.Exists(engineExe))
            {
                onFinished(false, "İndirme motoru bulunamadı. Lütfen internet bağlantınızı kontrol edip tekrar deneyin.", null);
                return null;
            }

            string outTemplate = Path.Combine(outputDirectory, "%(title)s.%(ext)s");

            string formatArgs;
            switch (format)
            {
                case DownloadFormat.Standard720:
                    formatArgs = "-f \"best[height<=720]/best\"";
                    break;
                case DownloadFormat.AudioMp3:
                    formatArgs = "-x --audio-format mp3 --audio-quality 0";
                    break;
                default: // BestVideo
                    formatArgs = "-f \"bestvideo[ext=mp4]+bestaudio[ext=m4a]/bestvideo+bestaudio/best\"";
                    break;
            }

            string arguments = string.Format("--no-warnings --newline --progress {0} -o \"{1}\" \"{2}\"", formatArgs, outTemplate, url);

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = engineExe,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            Process proc = new Process { StartInfo = psi, EnableRaisingEvents = true };

            string downloadedFilePath = null;

            proc.OutputDataReceived += (s, e) =>
            {
                if (string.IsNullOrEmpty(e.Data)) return;

                string line = e.Data.Trim();

                // Check for destination filename
                if (line.StartsWith("[download] Destination:") || line.StartsWith("[ExtractAudio] Destination:") || line.StartsWith("[Merger] Merging formats into"))
                {
                    Match mFile = Regex.Match(line, @"Destination:\s*(.+)$|into\s*""(.+)""");
                    if (mFile.Success)
                    {
                        downloadedFilePath = mFile.Groups[1].Success ? mFile.Groups[1].Value.Trim() : mFile.Groups[2].Value.Trim();
                    }
                }

                // Check for progress percentage
                Match mProg = Regex.Match(line, @"\[download\]\s+(\d+(?:\.\d+)?)%\s+of\s+~?(\S+)\s+at\s+(\S+)\s+ETA\s+(\S+)");
                if (mProg.Success)
                {
                    double pct;
                    double.TryParse(mProg.Groups[1].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out pct);

                    DownloadProgressArgs args = new DownloadProgressArgs
                    {
                        Percentage = pct,
                        TotalSize = mProg.Groups[2].Value,
                        Speed = mProg.Groups[3].Value,
                        Eta = mProg.Groups[4].Value,
                        StatusText = string.Format("%{0:F1} • {1} • Kalan: {2}", pct, mProg.Groups[3].Value, mProg.Groups[4].Value)
                    };
                    onProgress(args);
                }
            };

            string errorOutput = "";
            proc.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data)) errorOutput += e.Data + "\n";
            };

            proc.Exited += (s, e) =>
            {
                if (proc.ExitCode == 0)
                {
                    onFinished(true, null, downloadedFilePath);
                }
                else
                {
                    string msg = "İndirme tamamlanamadı.";
                    if (!string.IsNullOrEmpty(errorOutput))
                    {
                        if (errorOutput.Contains("Video unavailable")) msg = "Bu video artık mevcut değil veya gizli.";
                        else if (errorOutput.Contains("Private video")) msg = "Bu video gizli olarak ayarlanmış.";
                        else msg = errorOutput.Trim();
                    }
                    onFinished(false, msg, null);
                }
            };

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            return proc;
        }
    }
}
