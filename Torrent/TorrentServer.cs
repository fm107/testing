using System;
using System.Diagnostics;
using Serilog;

namespace WebTorrent.Torrent
{
    public class TorrentServer
    {
        private static readonly ILogger Log = new LoggerConfiguration().WriteTo.File("wwwroot/logs/TorrentServer.txt").CreateLogger();

        public static void Start()
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    //Arguments = @"-c 'cd /app/utorrent-server/ && ./utserver -settingspath utserver.conf -logfile /app/heroku_output/wwwroot/uploads/log.txt -daemon'",
                    FileName = "/app/utorrent-server/utserver",
                    Arguments =
                        "-configfile /app/utorrent-server/utserver.conf -logfile /app/heroku_output/wwwroot/logs/utorrent_log.txt -daemon",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true
                };

                Process.Start(processInfo)
                    .StandardOutput.ReadToEndAsync()
                    .ContinueWith(response => { Log.Information(response.Result); });
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }
        }
    }
}