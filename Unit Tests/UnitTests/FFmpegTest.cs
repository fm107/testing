using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebTorrent;

namespace UnitTests
{
    [TestClass]
    public class FFmpegTest
    {
        private const string FiletoConvert = @"D:\Temp\Logan.2017.BDRip(AVC).mkv";

        [TestMethod]
        public void CreatePlayListProcess()
        {
            var factoryLogger = new LoggerFactory();
            factoryLogger.CreateLogger<FFmpegTest>();
            var logger = new Logger<FFmpeg>(factoryLogger);
            var ffmpeg = new FFmpeg(new FFmpegOptions(), logger);
            ffmpeg.CreatePlayList(FiletoConvert, @"D:\Temp\New folder\ffmpeg-20170628-c1d1274-win32-static\bin\output",
                "output");

            Console.WriteLine("Ok");
        }
    }
}