using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Jil;

namespace SharedClasses.Messaging
{
    public class EventsAwaiter
    {
        private ConcurrentDictionary<string, Action<string>> Callbacks = new ConcurrentDictionary<string, Action<string>>();
        private TimeSpan timeout;

        public EventsAwaiter()
        {
            timeout = TimeSpan.FromSeconds(15);
        }

        public Task<T> AwaitResponse<T>(string flowId)
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

        public Task<T> AwaitResponse<T>(string flowId, Action action)
        {
            var awaitTask = AwaitResponse<T>(flowId);
            action();
            return awaitTask;
        }

        public void BindConsumer(IConsumer consumer)
        {
            consumer.Received += MessageHandler;
        }

        private void MessageHandler(object sender, MqMessage message)
        {
            if (message.FlowId != null)
            {
                this.Callbacks.TryRemove(message.FlowId, out var removed);
                removed?.Invoke(message.Data);
            }
        }
    }
}