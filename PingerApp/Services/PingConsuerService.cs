using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PingerApp.Data.Entity;
using PingerApp.Model;
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

            var maxConcurrency = 500;
            var semaphore = new SemaphoreSlim(maxConcurrency);

            var consumer=new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body=ea.Body.ToArray();

                var message = Encoding.UTF8.GetString(body);
                var ipAddresses = JsonConvert.DeserializeObject<List<IPAdresses>>(message);
                var pingTasks = new List<Task>();

                foreach (var ip in ipAddresses)
                {
                    await semaphore.WaitAsync(); // Wait for an available slot

                    var pingTask = Task.Run(async () =>
                    {
                        try
                        {
                            var pingResult = await _pingHelper.Pinger(ip.IPAddress);

                            var pingRecord = new PingRecord
                            {
                                IPAddress = ip.IPAddress,
                                Status = pingResult.Status.ToString(),
                                Rtt = pingResult.RoundtripTime,
                                Time=DateTime.Now.ToUniversalTime()
                            };

                            // Serialize and publish the result
                            var resultMessage = JsonConvert.SerializeObject(pingRecord);
                            var resultHeaders = new Dictionary<string, object> { { "TaskType", "StoreResult" } };
                            _rabbitMqHelper.PublishMessage(exchangename, string.Empty, resultHeaders, resultMessage);

                            Console.WriteLine($"Processed IP: {pingRecord.IPAddress}, Status: {pingRecord.Status}, RTT: {pingRecord.Rtt}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing IP: {ip.IPAddress}. Exception: {ex.Message}");
                        }
                        finally
                        {
                            semaphore.Release(); // Release the slot
                        }
                    });

                    pingTasks.Add(pingTask);
                }

                // Wait for all tasks to complete
                await Task.WhenAll(pingTasks);

                Console.WriteLine("Batch processing completed.");
            };


            channel.BasicConsume(queue: queueName, autoAck: true, consumer:consumer);

            Console.WriteLine("Ping Consumer started listening.");
            Console.ReadLine();

        }

        }
    }
