using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace StreamBadger.Services
{
    public class ExtensionContentType
    {
        private static readonly Dictionary<string, string> Lookup = new(StringComparer.OrdinalIgnoreCase)
        {
            [".png"] = "image/png",
            [".gif"] = "image/gif",
            [".mp3"] = "audio/mpeg",
            [".jpg"] = "image/jpeg"
        };

        public static bool TryGet(string fileName, [NotNullWhen(true)] out string? contentType)
        {
            var extension = Path.GetExtension(fileName);
            return Lookup.TryGetValue(extension, out contentType);
        }
    }
}