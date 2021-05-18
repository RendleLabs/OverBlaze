using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using StreamBadger.Services;

namespace StreamBadger.Endpoints
{
    public static class ShowEndpoint
    {
        public static async Task Show(HttpContext context)
        {
            var controlBus = context.RequestServices.GetRequiredService<ControlBus>();
            if (!context.Request.RouteValues.TryGetValue("image", out var image))
            {
                context.Response.StatusCode = 404;
                return;
            }

            var command = new ToggleImage(image.ToString());

            await controlBus.AddAsync(command);
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        }
    }
}