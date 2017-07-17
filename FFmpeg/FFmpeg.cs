using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebTorrent
{
    public class FFmpeg
    {
        private readonly FFmpegCmdBuilder _builder;
        private readonly FFmpegSettings _ffmpegSettings;
        private readonly ILogger<FFmpeg> _log;

        public FFmpeg(IOptions<FFmpegSettings> ffmpegOptions, ILogger<FFmpeg> log)
        {
            _log = log;
            _ffmpegSettings = ffmpegOptions.Value;
            _builder = new FFmpegCmdBuilder();
        }

        public void CreatePlayList(string fileToConvert, string outputPath, string playList)
        {
            Task.Factory.StartNew(() => CreatePlayListProcess(fileToConvert, outputPath, playList, false))
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
                        ? @"-i ""{0}"" -map 0:0 -map 0:1 -codec copy -f segment -segment_list_type m3u8 -segment_time 10 -segment_format mpegts -segment_list_flags +live -segment_list ""{1}/{2}.m3u8"" ""{1}/{2}.%d.ts"""
                        : @"-i ""{0}"" -c:v:0 libx264 -c:a:0 aac -preset ultrafast -profile:v baseline -level 3.0 -threads 0 -force_key_frames ""expr:gte(t,n_forced*10)"" -f segment -segment_time 10 -segment_format mpegts -segment_list_flags +live -segment_list ""{1}/{2}.m3u8"" -segment_list_type m3u8 ""{1}/{2}.%d.ts""",
                    fileToConvert, outputPath, playList)
            };

            var process = Process.Start(processInfo);
            process.WaitForExit();
            return process;
        }

        public void GetStreams(string fileToConvert)
        {
            var processInfo = new ProcessStartInfo(_ffmpegSettings.FilePath)
            {
                Arguments = string.Format(@"-i ""{0}""", fileToConvert),
                RedirectStandardError = true
            };

            var process = Process.Start(processInfo);

            var streamLine = new Regex("(Stream #(.*))");
            var streamChannel = new Regex(@"(Stream #)(\d+)(\W+)(\d+)");
            var output = process.StandardError.ReadToEnd();

            foreach (Match match in streamLine.Matches(output))
            {
                if (match.Value.Contains("Video:"))
                {
                    var channel = streamChannel.Match(match.Value).Value.Split('#').Last();
                    _builder.VideoList.Add(channel, channel);
                }

                if (match.Value.Contains("Audio:"))
                {
                    var channel = streamChannel.Match(match.Value).Value.Split('#').Last();
                    _builder.AudioList.Add(channel, channel);
                }
                if (match.Value.Contains("Subtitle:"))
                {
                    var channel = streamChannel.Match(match.Value).Value.Split('#').Last();
                    _builder.SubtitleList.Add(channel, channel);
                }
            }
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