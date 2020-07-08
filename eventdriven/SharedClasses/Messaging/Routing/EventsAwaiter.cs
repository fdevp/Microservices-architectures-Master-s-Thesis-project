using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Jil;

namespace SharedClasses.Messaging
{
    public class EventsAwaiter
    {
        private readonly string serviceName;
        private ConcurrentDictionary<string, Action<string>> Callbacks = new ConcurrentDictionary<string, Action<string>>();
        private TimeSpan timeout;

        public EventsAwaiter(string serviceName)
        {
            timeout = TimeSpan.FromSeconds(15);
            this.serviceName = serviceName;
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
                this.Callbacks.TryRemove(message.FlowId, out var callback);

                Console.WriteLine($"Service='{serviceName}' FlowId='{message.FlowId}' Method='{message.Type}' Type='Start'");
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    callback?.Invoke(message.Data);
                }
                finally
                {
                    Console.WriteLine($"Service='{serviceName}' FlowId='{message.FlowId}' Method='{message.Type}' Type='End'");
                }
            }
        }
    }
}