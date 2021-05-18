using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Twitch;
using MessagePack;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using StreamBadgerLogin.Models;

namespace StreamBadgerLogin
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddSingleton<IConnectionMultiplexer>(provider =>
            {
                var user = Environment.GetEnvironmentVariable("REDISUSER");
                var password = Environment.GetEnvironmentVariable("REDISPASSWORD");
                var host = Environment.GetEnvironmentVariable("REDISHOST");
                var portVar = Environment.GetEnvironmentVariable("REDISPORT");
                
                if (int.TryParse(portVar, out int port))
                {
                    var endpoint = new DnsEndPoint(host, port);
                    return ConnectionMultiplexer.Connect(new ConfigurationOptions
                    {
                        User = user,
                        Password = password,
                        EndPoints = {endpoint}
                    });
                }

                return null;
            });

            var clientId = Configuration.GetValue<string>("Twitch:ClientID");
            var clientSecret = Configuration.GetValue<string>("Twitch:ClientSecret");

            services.AddAuthentication(options => { options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; })
                .AddCookie()
                .AddTwitch(options =>
                {
                    options.CorrelationCookie.HttpOnly = true;
                    options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                    
                    options.ClientId = clientId;
                    options.ClientSecret = clientSecret;
                    options.Scope.Add("openid");
                    options.Scope.Add("chat:edit");
                    options.Scope.Add("chat:read");
                    options.Scope.Add("channel:read:redemptions");

                    options.SaveTokens = true;

                    options.Events = new OAuthEvents
                    {
                        OnCreatingTicket = OAuthCallback.OnCreatingTicket
                    };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto
            });
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHttpsRedirection();
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}