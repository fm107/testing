using System.Diagnostics;
using System.IO;
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
            Task.Factory.StartNew(() => CreatePlayListProcess(fileToConvert, outputPath, playList, true))
                .ContinueWith(task =>
                {
                    if (task.Result.ExitCode != 0)
                        CreatePlayListProcess(fileToConvert, outputPath, playList, false);
                });
        }

        private Process CreatePlayListProcess(string fileToConvert, string outputPath, string playList, bool copyCodec)
        {
            var processInfo = new ProcessStartInfo(_ffmpegSettings.FilePath)
            {
                Arguments = string.Format(copyCodec
                        ? @"-i ""{0}"" -map 0 -c:a:0 aac -codec copy -preset ultrafast -profile:v baseline -level 3.0 -f segment -segment_list_type m3u8 -segment_time 10 -segment_format mpegts -segment_list_flags +live -segment_list ""{1}/{2}.m3u8"" ""{1}/{2}.%d.ts"""
                        : @"-i ""{0}"" -c:v:0 libx264 -c:a:0 aac -preset ultrafast -profile:v baseline -level 3.0 -f segment -segment_time 10 -segment_format mpegts -segment_list_flags +live -segment_list ""{1}/{2}.m3u8"" -segment_list_type m3u8 ""{1}/{2}.%d.ts""",
                    fileToConvert, outputPath, playList)
            };

            var process = Process.Start(processInfo);
            process.WaitForExit();
            return process;
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