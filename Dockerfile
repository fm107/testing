#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk AS build
WORKDIR /src
COPY ["WebTorrent.csproj", "."]
COPY ["TorrentClient/Torrent.Client.csproj", "TorrentClient/"]
RUN dotnet restore "WebTorrent.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "WebTorrent.csproj" -c Release -o /app/build

FROM build AS publish

RUN cat /etc/os-release
RUN curl -sL https://deb.nodesource.com/setup_12.x | sh - 
RUN apt install -y nodejs

RUN npm install --force
RUN cp -R node_modules/reflect-metadata/* node_modules/angular2-universal-polyfills/node_modules/reflect-metadata/
RUN node node_modules/webpack/bin/webpack.js --config webpack.config.vendor.js
RUN node node_modules/webpack/bin/webpack.js

RUN dotnet publish "WebTorrent.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebTorrent.dll"]
