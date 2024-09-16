using RabbitMQ.Client;

namespace DevFreela.Payments.API.Extensions
{
    public static class RabbitMQExtensions
    {
        public static IServiceCollection AddRabbitMQ(this IServiceCollection services, string hostName, string userName, string password)
        {
            services.AddSingleton(sp =>
            {
                var factory = new ConnectionFactory()
                {
                    HostName = hostName,
                    UserName = userName,
                    Password = password
                };

                // Criação da conexão RabbitMQ
                var connection = factory.CreateConnection();

                // Criação do canal IModel
                return connection.CreateModel();
            });

            return services;
        }
    }
}
