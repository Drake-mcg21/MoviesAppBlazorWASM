// MovieEventConsumer.cs
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

public class MovieEventConsumer : BackgroundService
{
    private readonly ILogger<MovieEventConsumer> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    public MovieEventConsumer(ILogger<MovieEventConsumer> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            // Declare the exchange
            await _channel.ExchangeDeclareAsync(exchange: "movies", type: ExchangeType.Fanout);

            // Declare a queue and bind it to the exchange
            var queueDeclareResult = await _channel.QueueDeclareAsync(queue: "", durable: false, exclusive: true, autoDelete: true);
            var queueName = queueDeclareResult.QueueName;

            await _channel.QueueBindAsync(queue: queueName, exchange: "movies", routingKey: "");

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation("Received message: {Message}", message);

                    // Deserialize the message
                    var eventMessage = JsonSerializer.Deserialize<JsonElement>(message);
                    var eventName = eventMessage.GetProperty("Event").GetString();
                    var eventData = eventMessage.GetProperty("Data");

                    // Handle different event types
                    switch (eventName)
                    {
                        case "MovieCreated":
                            var movieId = eventData.GetProperty("Id").GetInt32();
                            var movieTitle = eventData.GetProperty("Title").GetString();
                            _logger.LogInformation("Movie created: ID={MovieId}, Title={MovieTitle}", movieId, movieTitle);
                            break;
                        case "MovieUpdated":
                            _logger.LogInformation("Movie updated: {EventData}", eventData.GetRawText());
                            break;
                        case "MovieDeleted":
                            _logger.LogInformation("Movie deleted: {EventData}", eventData.GetRawText());
                            break;
                        default:
                            _logger.LogWarning("Unknown event type: {EventName}", eventName);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                }
            };

            await _channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);

            // Keep the service running
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in MovieEventConsumer");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel?.IsOpen == true)
            await _channel.CloseAsync();
        if (_connection?.IsOpen == true)
            await _connection.CloseAsync();

        await base.StopAsync(cancellationToken);
    }
}