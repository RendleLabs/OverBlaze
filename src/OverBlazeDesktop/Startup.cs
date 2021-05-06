using System;
using Microsoft.Extensions.DependencyInjection;
using OverBlaze.Services;
using Photino.Blazor;

namespace OverBlazeDesktop
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // services.AddSingleton<ImageStore>();
            // services.AddSingleton<SoundStore>();
            // services.AddHttpClient<ServerClient>(client =>
            // {
            //     client.BaseAddress = new Uri("http://localhost:25293");
            // });
        }

        public void Configure(DesktopApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
