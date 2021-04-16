using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Blazored.Modal;
using Bot;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapGet("/images/{name}", ImageEndpoint.Get);
                endpoints.MapGet("/sounds/{name}", SoundEndpoint.Get);
                endpoints.MapGet("/show/{image}", ShowEndpoint.Show);
                endpoints.MapGet("/play/{sound}", PlayEndpoint.Play);
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
