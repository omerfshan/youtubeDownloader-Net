namespace Infrastructure.Services;
using YoutubeDownloader.Domain.Model;
using YoutubeDownloader.Aplication.Interface;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Diagnostics;

public class DownloaderService : IDownloaderService
{
    private string SanitizeFileName(string fileName)
    {
        string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
        string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
        return Regex.Replace(fileName, invalidRegStr, "_");
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.##} {sizes[order]}";
    }

    private async Task<string> RunYtDlpCommand(string arguments)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        var output = new List<string>();
        var error = new List<string>();

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                Console.WriteLine(e.Data);
                output.Add(e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                Console.WriteLine(e.Data);
                error.Add(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Exception($"yt-dlp hatası: {string.Join("\n", error)}");
        }

        return string.Join("\n", output);
    }

    public async Task<DownloadResult> DownloadVideoAsync(string url)
    {
        try 
        {
            // Video bilgilerini al
            var videoTitle = await RunYtDlpCommand($"--get-title {url}");
            Console.WriteLine($"Video başlığı: {videoTitle}");

            // İndirme seçeneklerini hazırla
            var options = new List<(string Label, string Format)>();
            int optionIndex = 1;

            // Video + Ses (En yüksek kalite)
            options.Add(("Video + Ses (En Yüksek Kalite)", "bestvideo*+bestaudio/best"));
            Console.WriteLine($"{optionIndex}. Video + Ses (En Yüksek Kalite)");

            // Sadece Video (En yüksek kalite)
            options.Add(("Sadece Video (En Yüksek Kalite)", "bestvideo/best"));
            Console.WriteLine($"{optionIndex + 1}. Sadece Video (En Yüksek Kalite)");

            // Sadece Ses (En yüksek kalite)
            options.Add(("Sadece Ses (En Yüksek Kalite)", "bestaudio/best"));
            Console.WriteLine($"{optionIndex + 2}. Sadece Ses (En Yüksek Kalite)");

            Console.Write("\nLütfen indirmek istediğiniz seçeneğin numarasını girin: ");
            if (!int.TryParse(Console.ReadLine(), out int selectedOption) || selectedOption < 1 || selectedOption > options.Count)
            {
                throw new Exception("Geçersiz seçim yapıldı.");
            }

            var selected = options[selectedOption - 1];
            string saveDirectory = Path.Combine(Directory.GetCurrentDirectory(), "SavedVideos");
            Directory.CreateDirectory(saveDirectory);

            var outputTemplate = Path.Combine(saveDirectory, "%(title)s.%(ext)s");
            var format = selected.Format;

            if (selected.Format.Contains("bestaudio"))
            {
                // Ses için ek parametreler
                await RunYtDlpCommand($"-f {format} -x --audio-format mp3 --audio-quality 0 -o \"{outputTemplate}\" {url}");
            }
            else
            {
                // Video için ek parametreler (FFmpeg ile birleştirme)
                var ffmpegLocation = await GetFFmpegLocation();
                var command = $"-f {format} --merge-output-format mp4 " +
                            $"--ffmpeg-location {ffmpegLocation} " +
                            $"--postprocessor-args \"-c:v copy -c:a aac -strict experimental -b:a 192k\" " +
                            $"-o \"{outputTemplate}\" " +
                            $"--verbose {url}";
                
                await RunYtDlpCommand(command);
            }

            // İndirilen dosyayı bul
            var downloadedFile = Directory.GetFiles(saveDirectory)
                .OrderByDescending(f => new FileInfo(f).CreationTime)
                .FirstOrDefault();

            if (downloadedFile == null)
            {
                throw new Exception("İndirilen dosya bulunamadı.");
            }

            var fileInfo = new FileInfo(downloadedFile);
            return new DownloadResult
            {
                FileName = Path.GetFileName(downloadedFile),
                FilePath = downloadedFile,
                FileSize = BitConverter.GetBytes(fileInfo.Length)
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hata detayı: {ex}");
            throw new Exception($"Video indirme hatası: {ex.Message}");
        }
    }

    private async Task<string> GetFFmpegLocation()
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "which",
                Arguments = "ffmpeg",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (string.IsNullOrWhiteSpace(output))
        {
            throw new Exception("FFmpeg bulunamadı. Lütfen FFmpeg'i yükleyin.");
        }

        return output.Trim();
    }
}