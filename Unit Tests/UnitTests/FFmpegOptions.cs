using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using WebTorrent;

namespace UnitTests
{
    class FFmpegOptions : IOptions<FFmpegSettings>
    {
        public FFmpegOptions()
        {
            Value = new FFmpegSettings
            {
                FilePath = @"D:\Temp\New folder\ffmpeg-20170628-c1d1274-win32-static\bin\ffmpeg.exe"
            };
        }
        public FFmpegSettings Value { get; }
    }
}
