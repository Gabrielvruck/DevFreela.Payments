﻿
using DevFreela.Payments.API.Models;
using DevFreela.Payments.API.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace DevFreela.Payments.API.Consumers
{
    public class ProcessPaymentConsumer : BackgroundService
    {
        private const string QUEUE = "Payments";
        private const string PAYMENT_APPROVED_QUEUE = "PaymentsApproved";
        private readonly IModel _channel;
        private readonly IServiceProvider _serviceProvider;

        public ProcessPaymentConsumer(IModel channel, IServiceProvider serviceProvider)
        {
            _channel = channel ?? throw new ArgumentNullException(nameof(channel));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            _channel.QueueDeclare(
                queue: QUEUE,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            _channel.QueueDeclare(
               queue: PAYMENT_APPROVED_QUEUE,
               durable: false,
               exclusive: false,
               autoDelete: false,
               arguments: null
           );
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (sender, eventArgs) =>
            {
                var byteArray = eventArgs.Body.ToArray();
                var paymentInfoJson = Encoding.UTF8.GetString(byteArray);
                var paymentInfo = JsonSerializer.Deserialize<PaymentInfoInputModel>(paymentInfoJson);
                ProcessPayment(paymentInfo);

                var paymentApproved = new PaymentApprovedIntegrationEvent(paymentInfo.Id);
                var paymentApprovedJson = JsonSerializer.Serialize(paymentApproved);
                var paymentApprovedBytes = Encoding.UTF8.GetBytes(paymentApprovedJson);

                _channel.BasicPublish(
                       exchange: "",
                       routingKey: PAYMENT_APPROVED_QUEUE,
                       basicProperties: null,
                       body: paymentApprovedBytes
                );

                _channel.BasicAck(eventArgs.DeliveryTag, false);
            };
            _channel.BasicConsume(QUEUE, false, consumer);
            return Task.CompletedTask;
        }

        public void ProcessPayment(PaymentInfoInputModel paymentInfo)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
                paymentService.Process(paymentInfo);
            }
        }
    }
}
