using System;
using Photino.Blazor;

namespace StreamBadgerDesktop
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var hostBuilder = StreamBadger.Program.CreateHostBuilder(Array.Empty<string>());
            using var api = hostBuilder.Build();
            api.StartAsync();

            try
            {
                ComponentsDesktop.Run<Startup>("StreamBadger"
                    , "wwwroot/index.html"
                    , x: 450
                    , y: 100
                    , width: 1600
                    , height: 900);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}