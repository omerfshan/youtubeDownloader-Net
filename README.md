# YouTube Video Downloader API

This project is a .NET Core API application for downloading YouTube videos.

## Features

- Asynchronous YouTube video downloading
- Get video files as byte arrays
- Simple and easy-to-use API interface

## Technical Requirements

- .NET Core 6.0 or higher
- Visual Studio 2022 or Visual Studio Code
- Internet connection

## Installation

1. Clone the repository:
```bash
git clone https://github.com/username/youtubeDownloaderApi.git
```

2. Navigate to the project directory:
```bash
cd youtubeDownloaderApi
```

3. Install dependencies:
```bash
dotnet restore
```

4. Run the project:
```bash
dotnet run
```

## Usage

You can use the `IDownloaderService` interface to interact with the API:

```csharp
public interface IDownloaderService
{
    Task<DownloadResult> DownloadVideoAsync(string url);
}
```

### Example Usage

```csharp
// Service injection
private readonly IDownloaderService _downloaderService;

// Download video
var result = await _downloaderService.DownloadVideoAsync("https://www.youtube.com/watch?v=VIDEO_ID");

// Save downloaded file
await File.WriteAllBytesAsync(result.FileName, result.FileBytes);
```

## Project Structure

- `Aplication/Interface`: Service interfaces
- `Domain/Model`: Data models
- `Infrastructure`: Infrastructure layer
- `Presentation`: API presentation layer

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## Support

If you encounter any issues or have questions, please open an issue in the GitHub repository.

## Acknowledgments

- Thanks to all contributors who have helped shape this project
- Special thanks to the YouTube API community for their support and resources
