using System;
using System.Collections.Generic;
using System.Text;

namespace BuildingBlocks.Messaging.Abstractions.Abstractions
{
    public interface IEventPublisher
    {
        Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class;
    }
}

