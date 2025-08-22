using Microsoft.EntityFrameworkCore;
using MoviesApp.Server.Data;
using MoviesApp.Server.Hubs;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// Add Entity Framework
builder.Services.AddDbContext<MoviesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add RabbitMQ Event Publisher
builder.Services.AddSingleton<IEventPublisher>(provider =>
{
    try
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        var connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        var channel = connection.CreateChannelAsync().GetAwaiter().GetResult();

        // Declare the exchange
        channel.ExchangeDeclareAsync(exchange: "movies", type: ExchangeType.Fanout).GetAwaiter().GetResult();

        return new RabbitMqEventPublisher(connection, channel);
    }
    catch (Exception ex)
    {
        var logger = provider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Failed to create RabbitMQ Event Publisher. Make sure RabbitMQ server is running.");
        throw;
    }
});

// Add the background service for consuming events
builder.Services.AddHostedService<MovieEventConsumer>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("https://localhost:7074", "http://localhost:58231", "http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowBlazorClient");
app.UseAuthorization();
app.MapControllers();
app.MapHub<MovieHub>("/moviehub");

app.Run();