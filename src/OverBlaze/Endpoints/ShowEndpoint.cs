using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OverBlaze.Services;

namespace OverBlaze.Endpoints
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

            var command = new ToggleImage
            {
                Path = "/Perry_the_platypus.png",
                Style = "height: 30vh; left: 40vw; top: 35vh; position: absolute;",
            };

            await controlBus.AddAsync(command);
            await context.Response.WriteAsync("OK");
            context.Response.StatusCode = 200;
        }
    }
}