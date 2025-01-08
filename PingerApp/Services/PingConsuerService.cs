using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PingerApp.Data.Entity;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PingerApp.Services
{
    public interface IPingConsumerService
    {
        void StartListening();
    }
    public class PingConsumerService:IPingConsumerService
    {
        private readonly IConfiguration _configuration;

        private readonly IPingHelper _pingHelper;

        private readonly IRabbitMQHelper _rabbitMqHelper;
        public PingConsumerService(IRabbitMQHelper rabbitMqHelper,IConfiguration configuration,IPingHelper pingHelper) 
        {
            _configuration = configuration;
            _pingHelper = pingHelper;
            _rabbitMqHelper = rabbitMqHelper;
        }

        public void StartListening()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQ:Host"],
                UserName = _configuration["RabbitMQ:Username"],
                Password = _configuration["RabbitMQ:Password"]
            };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            var exchangename = _configuration["RabbitMQ:ExchangeName"];
            channel.ExchangeDeclare(exchange: exchangename, type: ExchangeType.Headers);

            string queueName = channel.QueueDeclare().QueueName;
            var headers = new Dictionary<string, object> { { "TaskType", "Ping" }, { "x-match", "any" } };

            channel.QueueBind(queueName,exchangename,string.Empty,headers);

            var consumer=new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body=ea.Body.ToArray();

                var message = Encoding.UTF8.GetString(body);
                var ip = JsonConvert.DeserializeObject<IPAdresses>(message);

                var pingResult = await _pingHelper.Pinger(ip.IPAddress);

                var resultMessage = JsonConvert.SerializeObject(new
                {
                    IPAddress = ip.IPAddress,
                    Status = pingResult.Status.ToString(),
                    Rtt = pingResult.RoundtripTime
                });

                var ResultHeaders = new Dictionary<string, object> { { "TaskType", "StoreResult" } };
                _rabbitMqHelper.PublishMessage(exchangename, string.Empty, ResultHeaders, resultMessage);

                Console.WriteLine($"Processed IP: {ip.IPAddress}");
            };

            channel.BasicConsume(queue: queueName, autoAck: true, consumer:consumer);

            Console.WriteLine("Ping Consumer started listening.");
            Console.ReadLine();

        }

        }
    }
