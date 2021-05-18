using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using StreamBadger.Models;

namespace StreamBadger.Services
{
    public class SoundStore
    {
        private readonly ConcurrentDictionary<string, SoundModel?> _cache = new(StringComparer.OrdinalIgnoreCase);
        private readonly string _baseDirectory;

        public SoundStore()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _baseDirectory = Path.Combine(appData, "StreamBadger", "Sounds");
        }
        
        public async Task AddAsync(Stream stream, string contentType, UploadSoundModel model)
        {
            var directory = Path.Combine(_baseDirectory, model.Name);
            Directory.CreateDirectory(directory);
            
            var fileName = contentType.ToLower() switch
            {
                "audio/mpeg" => "sound.mp3",
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
            return Directory.EnumerateFiles(directory, "sound.*")
                .FirstOrDefault();
        }

        public IEnumerable<string> GetSoundNames()
        {
            foreach (var directory in Directory.EnumerateDirectories(_baseDirectory))
            {
                yield return Path.GetFileName(directory);
            }
        }

        public ValueTask<SoundModel?> GetSound(string name)
        {
            if (_cache.TryGetValue(name, out var soundModel))
            {
                return new ValueTask<SoundModel?>(soundModel);
            }
            
            var directory = Path.Combine(_baseDirectory, name);
            if (!Directory.Exists(directory))
            {
                return new ValueTask<SoundModel?>((SoundModel?) null);
            }

            return new ValueTask<SoundModel?>(CreateSoundModelAsync(directory, name));
        }

        private async Task<SoundModel?> CreateSoundModelAsync(string directory, string name)
        {
            var infoFile = Path.Combine(directory, "info.json");
            if (!File.Exists(infoFile)) return null;

            var soundFile = Directory.EnumerateFiles(directory, "sound.*").FirstOrDefault();
            if (soundFile == null) return null;

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

            var soundModel = new SoundModel(name, soundFile, model.Css);
            _cache.TryAdd(name, soundModel);
            return soundModel;
        }
    }
}