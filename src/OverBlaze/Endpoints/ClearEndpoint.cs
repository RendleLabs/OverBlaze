using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OverBlaze.Services;

namespace OverBlaze.Endpoints
{
    public static class ClearEndpoint
    {
        public static async Task Clear(HttpContext context)
        {
            var controlBus = context.RequestServices.GetRequiredService<ControlBus>();
            await controlBus.AddAsync(new ClearAll());
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        }
    }
}