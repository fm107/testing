using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using WebTorrent.Data;
using WebTorrent.Repository;
using WebTorrent.Services;

namespace WebTorrent
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var config = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UTorrent.Api.Data.Torrent, Model.TorrentInfo>();
            });

            var mapper = config.CreateMapper();
            services.AddSingleton(mapper);

            services.AddDbContext<ContentDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("Sqlite")));

            services.AddScoped<IContentRecordRepository, ContentRecordRepository>();

            services.AddSingleton<FsInfo, FsInfo>();
            services.AddSingleton<TorrentClient, TorrentClient>();

            services.AddSingleton<FFmpeg, FFmpeg>();

            // Configure FFmpeg using appsettings.json file.
            services.Configure<FFmpegSettings>(Configuration.GetSection("FFmpeg"));

            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (!Directory.Exists(Path.Combine(env.WebRootPath, "logs")))
            {
                Directory.CreateDirectory(Path.Combine(env.WebRootPath, "logs"));
            }

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.AddFile("wwwroot/logs/log.txt");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            // Set up custom content types -associating file extension to MIME type
            var provider = new FileExtensionContentTypeProvider();
            // Add new mappings
            provider.Mappings[".m3u8"] = "application/x-mpegURL";
            provider.Mappings[".woff"] = "application/x-font-woff";
            provider.Mappings[".woff2"] = "font/woff2";
            provider.Mappings[".ttf"] = "font/ttf";

            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = provider
            });

            app.UseDirectoryBrowser(new DirectoryBrowserOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(env.WebRootPath, "logs")),
                RequestPath = new PathString("/logs")
            });

            app.UseWebSockets();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    "spa-fallback",
                    new {controller = "Home", action = "Index"});
            });
        }
    }
}