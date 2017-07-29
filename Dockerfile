FROM microsoft/dotnet:1.1.2-sdk-jessie

RUN apt-get update
RUN wget -qO- https://deb.nodesource.com/setup_8.x | bash -
RUN apt-get install -y build-essential nodejs

COPY . /app

WORKDIR /app

RUN ["dotnet", "restore"]
RUN ["dotnet", "build", "/app/WebTorrent.csproj"]

EXPOSE 5000/tcp

CMD ["dotnet", "run", "--server.urls", "http://*:5000"]
