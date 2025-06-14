using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using YoutubeDownloader.Aplication.Interface;

namespace YouTubeDownloaderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DownloadController : ControllerBase
    {
        private readonly IDownloaderService _downloader;

        public DownloadController(IDownloaderService downloader)
        {
            _downloader = downloader;
        }

        [HttpGet]
        public async Task<IActionResult> Download([FromQuery] string url)
        {
            if (string.IsNullOrEmpty(url))
                return BadRequest("URL boş olamaz.");

            try
            {
                // Videoyu indirme işlemi
                var result = await _downloader.DownloadVideoAsync(url);

                // Dosya yolunu kullanarak içeriği okuma
                var fileBytes = await System.IO.File.ReadAllBytesAsync(result.FilePath);

                // Dosya içeriğini ve dosya adını döndür
                return File(fileBytes, "application/octet-stream", result.FileName);
            }
            catch (Exception ex)
            {
                // Hata durumunda mesaj döndür
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
