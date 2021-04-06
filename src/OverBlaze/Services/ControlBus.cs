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
                        if (DisplayImage is not null)
                        {
                            await DisplayImage.Invoke(image);
                        }
                        break;
                    case HideImage image:
                        if (HideImage is not null)
                        {
                            await HideImage.Invoke(image);
                        }
                        break;
                }
            }
        }

        public IAsyncEnumerable<object> ReadAsync(CancellationToken token)
        {
            return _channel.Reader.ReadAllAsync(token);
        }

        public event Func<ToggleImage, Task> DisplayImage;
        
        public event Func<HideImage, Task> HideImage;
    }
}