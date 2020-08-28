using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Jil;
using Microsoft.Extensions.Logging;
using SharedClasses.Events;

namespace SharedClasses.Messaging
{
    public class EventsAwaiter
    {
        private readonly string serviceName;
        private readonly ILogger logger;
        private ConcurrentDictionary<string, Action<MqMessage>> Callbacks = new ConcurrentDictionary<string, Action<MqMessage>>();
        private TimeSpan timeout;

        public EventsAwaiter(string serviceName, ILogger logger)
        {
            timeout = TimeSpan.FromSeconds(15);
            this.serviceName = serviceName;
            this.logger = logger;
        }

        public Task<T> AwaitResponse<T>(string flowId)
        {
            var cancellationToken = new CancellationTokenSource(timeout).Token;
            var tcs = new TaskCompletionSource<T>();

            this.Callbacks.TryAdd(flowId, (message) => Handle(tcs, message));

            cancellationToken.Register(() =>
            {
                this.Callbacks.TryRemove(flowId, out var removed);
                if (!tcs.Task.IsCompleted)
                {
					tcs.TrySetCanceled();
                    logger.LogInformation($"Service='{serviceName}' FlowId='{flowId}' Type='Timeout'");
                }
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
                callback?.Invoke(message);
            }
        }

        private void Handle<T>(TaskCompletionSource<T> tcs, MqMessage message)
        {
            if (message.Type == typeof(ErrorEvent).Name)
            {
                logger.LogInformation($"Service='{serviceName}' FlowId='{message.FlowId}' Method='{message.Type}' Type='Error'");
                var msg = (data as ErrorEvent)?.Message;
                tcs.SetException(new Exception(msg));
            }
            else
            {
				var data = JSON.Deserialize<T>(message.Data, Options.ISO8601Utc);
                tcs.SetResult(data);
            }
        }
    }
}