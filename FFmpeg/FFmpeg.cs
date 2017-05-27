using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebTorrent
{
    public class FFmpeg
    {
        private string _processPath = @"/app/vendor/ffmpeg/ffmpeg";

        public void CreatePlayList(string fileToConvert, string outputPath)
        {
            Task.Factory.StartNew(() =>
            {
                var processInfo = new ProcessStartInfo(_processPath)
                {
                    Arguments = string.Format(
                        @"-i {0} -codec:v libx264 -codec:a aac -map 0 -f segment -segment_time 10 -segment_format mpegts -segment_list_flags live -segment_list {1}/out.m3u8 -segment_list_type m3u8 {1}/%d.ts",
                        fileToConvert, outputPath)
                };

                var process = Process.Start(processInfo);
                process.WaitForExit();
            });
        }
    }
}
