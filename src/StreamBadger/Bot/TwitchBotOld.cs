using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StreamBadger.Services;
using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Core.RateLimiter;
using TwitchLib.Api.Helix;
using TwitchLib.Api.Services;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;

namespace Bot
{
    public class TwitchBotOld : BackgroundService
    {
        private readonly ControlBus _controlBus;
        private readonly ImageStore _imageStore;
        private readonly SoundStore _soundStore;
        private readonly TwitchAuthOld _twitchAuth;
        private readonly ILogger<TwitchBotOld> _logger;
        private TwitchClient _client;
        private TwitchPubSub _pubSub;
        private ConnectionCredentials _credentials;

        public TwitchBotOld(IConfiguration config, ControlBus controlBus,
            ImageStore imageStore, SoundStore soundStore,
            TwitchAuthOld twitchAuth, ILogger<TwitchBotOld> logger)
        {
            _controlBus = controlBus;
            _imageStore = imageStore;
            _soundStore = soundStore;
            _twitchAuth = twitchAuth;
            _logger = logger;

            _twitchAuth.TokenSet += TwitchAuthOnTokenSet;
        }

        private void TwitchAuthOnTokenSet()
        {
            var userName = _twitchAuth.UserName;
            var accessToken = _twitchAuth.Token;

            _credentials = new ConnectionCredentials(userName, accessToken);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient customClient = new(clientOptions);
            _client = new TwitchClient(customClient);
            _client.Initialize(_credentials, _twitchAuth.UserName.ToLowerInvariant());
            _client.OnMessageReceived += OnMessageReceived;
            _client.Connect();
            
            _pubSub = new TwitchPubSub();
            _pubSub.OnChannelPointsRewardRedeemed += OnChannelPointsRewardRedeemed;
            _pubSub.OnPubSubServiceError += OnPubSubServiceError;
            _pubSub.ListenToChannelPoints(_twitchAuth.UserId);
            _pubSub.Connect();
            _pubSub.SendTopics(accessToken);
        }

        private void OnPubSubServiceError(object? sender, OnPubSubServiceErrorArgs e)
        {
            _logger.LogError(e.Exception, e.Exception.Message);
        }

        private void OnChannelPointsRewardRedeemed(object? sender, OnChannelPointsRewardRedeemedArgs e)
        {
            _logger.LogInformation($"Reward redeemed: {e.RewardRedeemed.Redemption.Reward.Title}");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var completionSource = new TaskCompletionSource();
            stoppingToken.Register(() => completionSource.SetResult());
            return completionSource.Task;
        }

        private async void OnMessageReceived(object? sender, OnMessageReceivedArgs e)
        {
            if (TryGetCommand(e.ChatMessage.Message, out var command))
            {
                if (command.Equals("images"))
                {
                    var images = _imageStore.GetImageNames().Select(s => $"!{s}");
                    var text = string.Join(", ", images);
                    var message = $"These image commands are available: {text}";
                    _client.SendMessage(_twitchAuth.UserName.ToLowerInvariant(), message);
                    return;
                }
                var image = await _imageStore.GetImage(command);
                if (image is not null)
                {
                    var toggleImage = new ToggleImage(image.Name);
                    await _controlBus.AddAsync(toggleImage);
                    return;
                }

                var sound = await _soundStore.GetSound(command);
                if (sound is not null)
                {
                    var playSound = new PlaySound(sound.Name);
                    await _controlBus.AddAsync(playSound);
                    return;
                }
            }
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