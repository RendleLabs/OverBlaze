using System;
using Photino.Blazor;

namespace OverBlazeDesktop
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var hostBuilder = OverBlaze.Program.CreateHostBuilder(Array.Empty<string>());
            using var api = hostBuilder.Build();
            api.StartAsync();

            try
            {
                ComponentsDesktop.Run<Startup>("OverBlaze"
                    , "wwwroot/index.html"
                    , x: 450
                    , y: 100
                    , width: 1000
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