using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebTorrent
{
    public class FFmpegCmdBuilder
    {
        public FFmpegCmdBuilder()
        {
            VideoList=new Dictionary<string, string>();
            AudioList=new Dictionary<string, string>();
            SubtitleList=new Dictionary<string, string>();
        }
        public Dictionary<string,string> VideoList { get; set; }
        public Dictionary<string, string> AudioList { get; set; }
        public Dictionary<string, string> SubtitleList { get; set; }
    }
}
