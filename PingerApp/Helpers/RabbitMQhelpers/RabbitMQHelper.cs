using System.Text;
using Microsoft.Extensions.Configuration;
using PingerApp.Model;
using RabbitMQ.Client;

namespace PingerApp.Helpers.RabbitMQhelpers
{
    public interface IRabbitMQHelper
    {
        void PublishMessage(string exchange, string routingKey, Dictionary<string, object> headers, string message);
    }
    public class RabbitMQHelper : IRabbitMQHelper
    {
        private readonly IConfiguration _configuration;
        private readonly RabbitMQSettings _rabbitMQSettings;
        private readonly IRabbitMQConnectionManager _connectionManager;
        public RabbitMQHelper(IConfiguration configuration, RabbitMQSettings rabbitMQSettings, IRabbitMQConnectionManager rabbitMQConnectionManager)
        {
            _configuration = configuration;
            _rabbitMQSettings = rabbitMQSettings;
            _connectionManager = rabbitMQConnectionManager;
        }



        public void PublishMessage(string exchange, string routingKey, Dictionary<string, object> headers, string message)
        {



            var channel = _connectionManager.CreateChannel();

            var properties = channel.CreateBasicProperties();
            properties.Headers = headers;
            properties.Persistent = true;

            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange, routingKey, properties, body);
        }

    }
}
