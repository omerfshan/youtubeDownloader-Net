//Task = “Şu işi arka planda yap, bitince bana sonucu getir.”
//“Bu metot, arka planda çalışır ve işi bittiğinde bana bir DownloadResult döner.”
/*DownloadVideoAsync: Asenkron olarak (yani uygulamayı kilitlemeden) bir video indirme işlemi yapar.
Task<DownloadResult>: Bu işlem tamamlandığında, içinde indirilen videonun dosya adı ve baytları (FileName ve FileBytes) olan bir DownloadResult döner.*/
namespace YoutubeDownloader.Aplication.Interface;
using YoutubeDownloader.Domain.Model;
public interface IDownloaderService
{
    Task<DownloadResult> DownloadVideoAsync(string url);
}
