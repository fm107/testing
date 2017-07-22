using System.Diagnostics;
using System.IO;
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
            Task.Factory.StartNew(() =>
            {
                _ffmpeg = FFmpegBuilder.CreateFFmpegBuilder(_ffmpegSettings.FilePath, fileToConvert)
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

        //Todo review
        private void ConvertVideo(object state)
        {
            Task.Factory.StartNew(() =>
            {
                var fileToConvert = ""; //Path.Combine(tor.Path, file.Name);

                var processInfo = new ProcessStartInfo("/app/vendor/ffmpeg/ffmpeg")
                {
                    Arguments = string.Format(@"-i {0} -f mp4 -vcodec libx264 -preset ultrafast 
                                                                                     -movflags faststart -profile:v main -acodec aac {1} -hide_banner",
                        fileToConvert,
                        string.Format("{0}.mp4", Path.ChangeExtension(fileToConvert, null)))
                };

                var process = Process.Start(processInfo);
                process.WaitForExit();
                //File.Delete(fileToConvert);
            });
        }
    }
}