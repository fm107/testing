using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace WebTorrent
{
    public class FFmpeg
    {
        private readonly FFmpegSettings _ffmpegSettings;

        public FFmpeg(IOptions<FFmpegSettings> ffmpegOptions)
        {
            _ffmpegSettings = ffmpegOptions.Value;
        }

        public void CreatePlayList(string fileToConvert, string outputPath, string playList)
        {
            Task.Factory.StartNew(() =>
            {
                var processInfo = new ProcessStartInfo(_ffmpegSettings.FilePath)
                {
                    Arguments = string.Format(
                        @"-i {0} -codec:v libx264 -codec:a aac -map 0 -f segment -segment_time 10 -segment_format mpegts -segment_list_flags live -segment_list {1}/{2}.m3u8 -segment_list_type m3u8 {1}/{2}.%d.ts",
                        fileToConvert, outputPath, playList)
                };

                var process = Process.Start(processInfo);
                process.WaitForExit();
            });
        }
    }
}
