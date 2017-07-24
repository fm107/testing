using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebTorrent
{
    public class FFmpeg
    {
        private readonly FFmpegSettings _ffmpegSettings;
        private readonly ILogger<FFmpeg> _log;
        private FFmpegArguments _ffmpeg;

        public FFmpeg(IOptions<FFmpegSettings> ffmpegOptions, ILogger<FFmpeg> log)
        {
            _log = log;
            _ffmpegSettings = ffmpegOptions.Value;
        }

        public void CreatePlayList(string fileToConvert, string outputPath, string playList)
        {
            _ffmpeg = new FFmpegBuilder(_ffmpegSettings.FilePath, fileToConvert)
                .MapVideoStream(1)
                .MapAudioStream(1)
                .CreateOnlinePlayList(outputPath, playList)
                .Build();

            Task.Factory.StartNew(() =>
            {
                _ffmpeg = new FFmpegBuilder(_ffmpegSettings.FilePath, fileToConvert)
                    .MapVideoStream(1)
                    .MapAudioStream(1)
                    .CreateOnlinePlayList(outputPath, playList)
                    .Build();

                var processInfo = new ProcessStartInfo(_ffmpegSettings.FilePath)
                {
                    Arguments = _ffmpeg.CmdArguments,
                    RedirectStandardOutput = true
                };

                using (var process = Process.Start(processInfo))
                {
                    process.WaitForExit();
                    var output = process.StandardOutput.ReadToEnd();
                    _log.LogInformation(output);
                }
            });
        }
    }
}