using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace PingerApp.Services
{
    public interface IRabbitMQHelper
    {
        void PublishMessage(string exchange, string routingKey, Dictionary<string, object> headers, string message);
    }
    public class RabbitMQHelper:IRabbitMQHelper
    {
        private readonly IConfiguration _configuration;
        public RabbitMQHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void PublishMessage(string exchange, string routingKey, Dictionary<string, object> headers, string message)
        {
                var factory = new ConnectionFactory()
                {
                    HostName = _configuration["RabbitMQ:Host"],
                    UserName = _configuration["RabbitMQ:Username"],
                    Password = _configuration["RabbitMQ:Password"]
                };
                var connection = factory.CreateConnection();
                var channel = connection.CreateModel();

            var properties=channel.CreateBasicProperties();
            properties.Headers=headers;
            properties.Persistent = true;

            var body=Encoding.UTF8.GetBytes(message);   

            channel.BasicPublish(exchange, routingKey, properties, body);
        }

    }
}
