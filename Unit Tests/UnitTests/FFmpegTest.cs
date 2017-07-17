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
        private string filetoConvert =
                @"D:\Temp\Logan.2017.BDRip(AVC).mkv";

        [TestMethod]
        public void CreatePlayListProcess()
        {
            var factoryLogger= new LoggerFactory();
            factoryLogger.AddFile(@"D:\Temp\New folder\ffmpeg-20170628-c1d1274-win32-static\bin\output\");
            var logger = new Logger<FFmpeg>(factoryLogger);
            var ffmpeg = new FFmpeg(new FFmpegOptions(), logger);
            ffmpeg.CreatePlayList(filetoConvert, @"D:\Temp\New folder\ffmpeg-20170628-c1d1274-win32-static\bin\output", "output");
            
            Thread.Sleep(TimeSpan.FromSeconds(30));
            Console.WriteLine("Ok123");
        }
        [TestMethod]
        public void CreatePlayList()
        {
            var factoryLogger= new LoggerFactory();
            factoryLogger.AddFile(@"D:\Temp\New folder\ffmpeg-20170628-c1d1274-win32-static\bin\output\log.txt");
            var logger = new Logger<FFmpeg>(factoryLogger);
            var ffmpeg = new FFmpeg(new FFmpegOptions(), logger);
            ffmpeg.GetStreams(filetoConvert);
            
            Console.WriteLine("Ok123");
        }
    }
}
