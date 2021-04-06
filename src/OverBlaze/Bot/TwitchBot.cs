using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OverBlaze.Services;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace Bot
{
    public class TwitchBot : BackgroundService
    {
        private readonly ControlBus _controlBus;
        private readonly ImageStore _imageStore;
        private readonly TwitchClient _client;
        private readonly ConnectionCredentials _credentials;

        public TwitchBot(IConfiguration config, ControlBus controlBus, ImageStore imageStore)
        {
            _controlBus = controlBus;
            _imageStore = imageStore;

            var userName = config.GetValue<string>("Twitch:UserName");
            var accessToken = config.GetValue<string>("Twitch:AccessToken");

            _credentials = new ConnectionCredentials(userName, accessToken);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            _client = new TwitchClient(customClient);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _client.Initialize(_credentials, "markrendle");

            _client.OnMessageReceived += OnMessageReceived;

            _client.Connect();

            var completionSource = new TaskCompletionSource();
            stoppingToken.Register(() => completionSource.SetResult());
            return completionSource.Task;
        }

        private async void OnMessageReceived(object? sender, OnMessageReceivedArgs e)
        {
            if (TryGetCommand(e.ChatMessage.Message, out var command))
            {
                var image = await _imageStore.GetImage(command);
                if (image is not null)
                {
                    var toggleImage = new ToggleImage
                    {
                        Path = $"/{image.Name}",
                        Style = "height: 30vh; left: 40vw; top: 35vh; position: absolute;",
                    };

                    await _controlBus.AddAsync(toggleImage);
                }
            }
            // if (e.ChatMessage.Message.Equals("!perry", StringComparison.OrdinalIgnoreCase))
            // {
            //     var command = new ToggleImage
            //     {
            //         Path = "/Perry_the_platypus.png",
            //         Style = "height: 30vh; left: 40vw; top: 35vh; position: absolute;",
            //     };
            //
            //     await _controlBus.AddAsync(command);
            // }
        }

        private bool TryGetCommand(ReadOnlySpan<char> text, [NotNullWhen(true)] out string? command)
        {
            command = null;
            if (text is not {Length: > 0}) return false;

            if (text[0] != '!') return false;

            text = text.Slice(1);
            
            foreach (char c in text)
            {
                if (char.IsLetterOrDigit(c)) continue;
                if (c is '_' or '-') continue;
                return false;
            }

            command = new string(text);
            return true;
        }
    }
}