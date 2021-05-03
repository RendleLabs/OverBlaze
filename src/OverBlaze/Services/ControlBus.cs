using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace OverBlaze.Services
{
    public class ControlBus
    {
        private readonly Channel<object> _channel;
        private readonly Task _run;

        public ControlBus()
        {
            _channel = Channel.CreateUnbounded<object>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false
            });
            _run = RunAsync();
        }

        public async Task AddAsync(object obj)
        {
            await _channel.Writer.WriteAsync(obj);
        }

        private async Task RunAsync()
        {
            await foreach (var item in _channel.Reader.ReadAllAsync())
            {
                switch (item)
                {
                    case ToggleImage image:
                        if (ToggleImage is not null)
                        {
                            await ToggleImage.Invoke(image);
                        }
                        break;
                    case PlaySound sound:
                        if (PlaySound is not null)
                        {
                            await PlaySound.Invoke(sound);
                        }
                        break;
                    case Services.ClearAll:
                        if (ClearAll is not null)
                        {
                            await ClearAll.Invoke();
                        }
                        break;
                }
            }
        }

        public IAsyncEnumerable<object> ReadAsync(CancellationToken token)
        {
            return _channel.Reader.ReadAllAsync(token);
        }

        public event Func<ToggleImage, Task> ToggleImage;
        public event Func<PlaySound, Task> PlaySound;
        public event Func<Task> ClearAll;
    }
}