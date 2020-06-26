using System;
using System.Collections.Concurrent;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jil;
using SharedClasses.Messaging;

namespace APIGateway.Logic
{
    public class EventsAwaiter
    {
        private ConcurrentDictionary<string, Action<string>> Callbacks = new ConcurrentDictionary<string, Action<string>>();
        private TimeSpan timeout;

        public EventsAwaiter()
        {
            timeout = TimeSpan.FromSeconds(30);
        }

        public Task<T> AwaitResponse<T>(string flowId) where T : class
        {
            var cancellationToken = new CancellationTokenSource(timeout).Token;
            var tcs = new TaskCompletionSource<T>();

            this.Callbacks.TryAdd(flowId, (message) =>
            {
                var data = JSON.Deserialize<T>(message);
                tcs.SetResult(data);
            });

            cancellationToken.Register(() =>
            {
                this.Callbacks.TryRemove(flowId, out var removed);
                tcs.TrySetCanceled();
            });

            return tcs.Task;
        }

        public void MessageHandler(object sender, EventHand message)
        {
            this.Callbacks.TryRemove(message.FlowId, out var removed);
            removed?.Invoke(message.Data);
        }
    }
}
