using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using StreamBadger.Services;

namespace StreamBadger.Endpoints
{
    public static class PlayEndpoint
    {
        public static async Task Play(HttpContext context)
        {
            var controlBus = context.RequestServices.GetRequiredService<ControlBus>();
            if (!context.Request.RouteValues.TryGetValue("sound", out var sound))
            {
                context.Response.StatusCode = 404;
                return;
            }

            var command = new PlaySound(sound.ToString());

            await controlBus.AddAsync(command);
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        }
    }
}