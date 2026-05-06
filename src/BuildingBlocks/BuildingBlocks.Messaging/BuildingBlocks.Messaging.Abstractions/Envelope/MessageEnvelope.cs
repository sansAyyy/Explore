namespace BuildingBlocks.Messaging.Abstractions.Envelope
{
    public class MessageEnvelope<T>
    {
        public Guid MessageId { get; set; } = Guid.NewGuid();
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
        public string? CorrelationId { get; set; }
        public T Payload { get; set; } = default!;

        public static MessageEnvelope<T> Create(T payload)
        {
            return Create(payload, Guid.NewGuid(), DateTime.UtcNow, null);
        }

        public static MessageEnvelope<T> Create(
            T payload,
            Guid messageId,
            DateTime occurredOn,
            string? correlationId)
        {
            ArgumentNullException.ThrowIfNull(payload);

            return new MessageEnvelope<T>
            {
                MessageId = messageId,
                OccurredOn = occurredOn,
                CorrelationId = correlationId,
                Payload = payload,
            };
        }
    }
}

