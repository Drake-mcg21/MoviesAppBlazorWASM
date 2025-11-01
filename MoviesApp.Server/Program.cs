using Microsoft.EntityFrameworkCore;
using MoviesApp.Server.Data;
using MoviesApp.Server.Hubs;
using MoviesApp.Server.Protos;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddGrpc();
// Add Entity Framework
builder.Services.AddDbContext<MoviesDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

builder.Services.AddGrpcClient<MoviesService.MoviesServiceClient>(options =>
{
    // Configure to call localhost gRPC service
    options.Address = new Uri("https://localhost:7001"); // Adjust port as needed
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