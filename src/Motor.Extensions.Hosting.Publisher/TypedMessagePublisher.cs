using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Motor.Extensions.Conversion.Abstractions;
using Motor.Extensions.Diagnostics.Metrics.Abstractions;
using Motor.Extensions.Hosting.Abstractions;
using Prometheus.Client.Abstractions;

namespace Motor.Extensions.Hosting.Publisher
{
    public class TypedMessagePublisher<TOutput, TPublisher> : ITypedMessagePublisher<TOutput>
        where TPublisher : ITypedMessagePublisher<byte[]>
        where TOutput : class
    {
        private readonly TPublisher _bytesMessagePublisher;
        private readonly ISummary? _messageSerialization;
        private readonly IMessageSerializer<TOutput> _messageSerializer;

        public TypedMessagePublisher(IMetricsFactory<TypedMessagePublisher<TOutput, TPublisher>>? metrics,
            TPublisher bytesMessagePublisher, IMessageSerializer<TOutput> messageSerializer)
        {
            _bytesMessagePublisher = bytesMessagePublisher;
            _messageSerializer = messageSerializer;
            _messageSerialization =
                metrics?.CreateSummary("message_serialization", "Message serialization duration in ms");
        }

        public async Task PublishMessageAsync(MotorCloudEvent<TOutput> cloudEvent, CancellationToken token = default)
        {
            var watch = Stopwatch.StartNew();
            byte[] bytes;
            try
            {
                bytes = _messageSerializer.Serialize(cloudEvent.TypedData);
            }
            finally
            {
                watch.Stop();
                _messageSerialization?.Observe(watch.ElapsedMilliseconds);
            }

            var bytesEvent = cloudEvent.CreateNew(bytes, true);
            await _bytesMessagePublisher.PublishMessageAsync(bytesEvent, token);
        }
    }
}
