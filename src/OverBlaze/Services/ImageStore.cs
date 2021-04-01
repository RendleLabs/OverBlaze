using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder.Extensions;
using OverBlaze.Models;

namespace OverBlaze.Services
{
    public class ImageStore
    {
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
    }
}