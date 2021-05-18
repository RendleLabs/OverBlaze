using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using StreamBadger.Services;

namespace StreamBadger.Endpoints
{
    public static class ImageEndpoint
    {
        public static async Task Get(HttpContext context)
        {
            var name = context.Request.RouteValues["name"] as string;
            if (name is null)
            {
                context.Response.StatusCode = 404;
                return;
            }

            var imageStore = context.RequestServices.GetRequiredService<ImageStore>();
            var path = imageStore.GetPath(name);
            if (ExtensionContentType.TryGet(path, out var contentType))
            {
                context.Response.StatusCode = 200;
                context.Response.Headers[HeaderNames.ContentType] = contentType;
                context.Response.Headers[HeaderNames.ContentLength] = new FileInfo(path).Length.ToString();
                await using (var stream = File.OpenRead(path))
                {
                    await stream.CopyToAsync(context.Response.Body);
                }

                return;
            }

            context.Response.StatusCode = 404;
        }
    }
}