﻿using System;
using System.Diagnostics;
using System.Reflection;
using log4net;

namespace WebTorrent.Torrent
{
    public class TorrentServer
    {
        private static readonly ILog Log;

        static TorrentServer()
        {
            Log = LogManager.GetLogger(Assembly.GetEntryAssembly(), "uTorrent");
        }

        public static void Start()
        {
            try
            {
                var processInfo = new ProcessStartInfo("/app/utorrent-server/utorrent.sh")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true
                };

                Process.Start(processInfo)
                    .StandardOutput.ReadToEndAsync()
                    .ContinueWith(response => { Log.Info(response.Result); });
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}