using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OverBlaze.Models;

namespace OverBlaze.Services
{
    public class ImageStore
    {
        private readonly Dictionary<string, ImageModel?> _cache = new(StringComparer.OrdinalIgnoreCase);
        private readonly string _baseDirectory;

        public ImageStore()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _baseDirectory = Path.Combine(appData, "OverBlaze", "Images");
        }
        
        public async Task AddAsync(Stream stream, string contentType, UploadImageModel model)
        {
            var directory = Path.Combine(_baseDirectory, model.Name);
            Directory.CreateDirectory(directory);
            var fileName = contentType.ToLower() switch
            {
                "image/png" => "image.png",
                "image/gif" => "image.gif",
                "image/jpeg" => "image.jpg",
                _ => null,
            };

            if (fileName is null) return;

            var filePath = Path.Combine(directory, fileName);
            await using (var file = File.Create(filePath))
            {
                await stream.CopyToAsync(file);
            }

            var infoPath = Path.Combine(directory, "info.json");
            await using (var infoFile = File.Create(infoPath))
            {
                await JsonSerializer.SerializeAsync(infoFile, model);
            }
        }

        public string? GetPath(string name)
        {
            var directory = Path.Combine(_baseDirectory, name);
            if (!Directory.Exists(directory)) return null;
            return Directory.EnumerateFiles(directory, "image.*")
                .FirstOrDefault();
        }

        public IEnumerable<string> GetImageNames()
        {
            foreach (var directory in Directory.EnumerateDirectories(_baseDirectory))
            {
                yield return Path.GetFileName(directory);
            }
        }

        public ValueTask<ImageModel?> GetImage(string name)
        {
            if (_cache.TryGetValue(name, out var imageModel))
            {
                return new ValueTask<ImageModel?>(imageModel);
            }
            
            var directory = Path.Combine(_baseDirectory, name);
            if (!Directory.Exists(directory))
            {
                return new ValueTask<ImageModel?>((ImageModel?) null);
            }

            return new ValueTask<ImageModel?>(CreateImageModelAsync(directory, name));
        }

        private async Task<ImageModel?> CreateImageModelAsync(string directory, string name)
        {
            var infoFile = Path.Combine(directory, "info.json");
            if (!File.Exists(infoFile)) return null;

            var imageFile = Directory.EnumerateFiles(directory, "image.*").FirstOrDefault();
            if (imageFile == null) return null;

            UploadImageModel? model;
            try
            {
                await using (var info = File.OpenRead(infoFile))
                {
                    model = await JsonSerializer.DeserializeAsync<UploadImageModel>(info);
                }
            }
            catch
            {
                return null;
            }

            if (model is null) return null;

            // "height: 30vh; left: 40vw; top: 35vh; position: absolute;",

            var builder = new StringBuilder("position: absolute;");
            if (model.Height.HasValue)
            {
                builder.Append($" height: {model.Height.Value}vh");
            }

            if (model.Width.HasValue)
            {
                builder.Append($" width: {model.Width.Value}vw");
            }

            if (model.Top.HasValue)
            {
                builder.Append($" top: {model.Top.Value}vh");
            }

            if (model.Left.HasValue)
            {
                builder.Append($" left: {model.Left.Value}vw");
            }

            var imageModel = new ImageModel(name, imageFile, builder.ToString());
            _cache[name] = imageModel;
            return imageModel;
        }
    }
}