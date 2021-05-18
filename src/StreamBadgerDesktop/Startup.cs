using System;
using Microsoft.Extensions.DependencyInjection;
using StreamBadger.Services;
using Photino.Blazor;
using StreamBadgerDesktop.Services;

namespace StreamBadgerDesktop
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ImageStore>();
            services.AddSingleton<SoundStore>();
            services.AddSingleton<TwitchAuth>();
            services.AddHostedService<TwitchBot>();
            
            services.AddHttpClient<ServerClient>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:25293");
            });

            services.AddHttpClient<LoginClient>(client =>
            {
                client.BaseAddress = new Uri("https://streambadger.com");
            });
        }

        public void Configure(DesktopApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
