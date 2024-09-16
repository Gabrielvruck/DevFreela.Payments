using DevFreela.Payments.API.Consumers;
using DevFreela.Payments.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configurando RabbitMQ via extensão
builder.Services.AddRabbitMQ("localhost", "guest", "guest");
builder.Services.AddInfrastructure();
builder.Services.AddHostedService<ProcessPaymentConsumer>();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
