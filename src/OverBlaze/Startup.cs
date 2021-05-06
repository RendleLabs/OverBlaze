using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Blazored.Modal;
using Bot;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using OBSWebsocketDotNet;
using OverBlaze.Data;
using OverBlaze.Endpoints;
using OverBlaze.Services;

namespace OverBlaze
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddBlazoredModal();
            services.AddBlazoredLocalStorage();
            
            services.AddSingleton<ControlBus>();
            services.AddSingleton<ImageStore>();
            services.AddSingleton<SoundStore>();

            services.AddHostedService<TwitchBot>();

            services.AddSingleton<TwitchAuth>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            IHostApplicationLifetime applicationLifetime)
        {
            // applicationLifetime.ApplicationStarted.Register(() => StartBrowser(app));
            applicationLifetime.ApplicationStarted.Register(() => ToggleOBSSource(true));
            applicationLifetime.ApplicationStopping.Register(() => ToggleOBSSource(false));
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapGet("/images/{name}", ImageEndpoint.Get);
                endpoints.MapGet("/sounds/{name}", SoundEndpoint.Get);
                endpoints.MapGet("/show/{image}", ShowEndpoint.Show);
                endpoints.MapGet("/play/{sound}", PlayEndpoint.Play);
                endpoints.MapGet("/clear", ClearEndpoint.Clear);
                endpoints.MapFallbackToPage("/_Host");
            });
        }

        private void StartBrowser(IApplicationBuilder app)
        {
            var addressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();
            var uri = addressesFeature?.Addresses.FirstOrDefault();
            if (uri is null) return;
            uri = $"{uri.Trim('/')}/Dashboard";
            ProcessStartInfo start;
            if (Configuration.GetValue<string>("Launch:Browser") is {Length: >0} browser)
            {
                start = new ProcessStartInfo
                {
                    FileName = browser,
                    Arguments = uri,
                    UseShellExecute = true
                };
            }
            else
            {
                start = new ProcessStartInfo
                {
                    FileName = uri,
                    UseShellExecute = true,
                };
            }

            Process.Start(start);
        }

        private void ToggleOBSSource(bool toggle)
        {
            OBSWebsocket obsWebsocket = new OBSWebsocket();
            var browserSourceName = "OverBlaze";
            var obsWebSocketUrl = Configuration.GetValue<string>("OBS:WebSocketUrl");
            var obsWebSocketPassword = Configuration.GetValue<string>("OBS:WebSocketPassword");
            if (obsWebSocketUrl is null) return;
            obsWebsocket.Connect(obsWebSocketUrl, obsWebSocketPassword);
            obsWebsocket.SetSourceRender(browserSourceName, toggle);
            if(toggle)
            {
                obsWebsocket.RefreshBrowserSource(browserSourceName);
            }
            obsWebsocket.Disconnect();
        }
    }
}
