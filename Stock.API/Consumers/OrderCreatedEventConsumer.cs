using Common.Shared.Events;
using MassTransit;
using System.Diagnostics;
using System.Text.Json;

namespace Stock.API.Consumers;

public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
{
    public Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {


        Thread.Sleep(2000);

        var a = Activity.Current;

        Activity.Current?.SetTag("message.body", JsonSerializer.Serialize(context.Message));

        return Task.CompletedTask;
    }
}
