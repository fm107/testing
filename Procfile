web: cd $HOME/heroku_output && dotnet WebTorrent.dll --server.urls http://0.0.0.0:$PORT

worker: /app/utorrent-server/utserver -configfile /app/utorrent-server/utserver.conf -logfile /app/heroku_output/wwwroot/uploads/log.txt -daemon