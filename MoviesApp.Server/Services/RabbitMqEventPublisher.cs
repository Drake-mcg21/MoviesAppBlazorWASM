using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
public interface IEventPublisher
{
    Task PublishAsync(string eventName, object payload, CancellationToken cancellationToken = default);
}


public class RabbitMqEventPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly string _exchangeName = "movies";

    public RabbitMqEventPublisher(IConnection connection, IChannel channel)
    {
        _connection = connection;
        _channel = channel;
    }

    public async Task PublishAsync(string eventName, object payload, CancellationToken cancellationToken = default)
    {
        var message = JsonSerializer.Serialize(new { Event = eventName, Data = payload });
        var body = Encoding.UTF8.GetBytes(message);

        var properties = new BasicProperties();

        await _channel.BasicPublishAsync(
            exchange: _exchangeName,
            routingKey: "",
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken
        );
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel?.IsOpen == true)
            await _channel.CloseAsync();
        if (_connection?.IsOpen == true)
            await _connection.CloseAsync();
    }
}


