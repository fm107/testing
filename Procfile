web: cd $HOME/heroku_output && dotnet WebTorrent.dll --server.urls http://0.0.0.0:$PORT
web: cd $HOME/utorrent-server && ./utserver -configfile utserver.conf -logfile /app/heroku_output/wwwroot/uploads/log.txt 
worker: cd $HOME/utorrent-server && ./utserver -configfile utserver.conf -logfile /app/heroku_output/wwwroot/uploads/log.txt 