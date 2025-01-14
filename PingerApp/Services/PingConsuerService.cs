using System.Diagnostics;
using System.Text;
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

        private readonly IDatabaseService _databaseService;
        public PingConsumerService(IRabbitMQHelper rabbitMqHelper,IConfiguration configuration,IPingHelper pingHelper,IDatabaseService databaseService) 
        {
            _configuration = configuration;
            _pingHelper = pingHelper;
            _rabbitMqHelper = rabbitMqHelper;
            _databaseService = databaseService;
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
            channel.ExchangeDeclare(exchange: exchangename, type: ExchangeType.Headers, durable: true, autoDelete: false);

            string queueName = _configuration["RabbitMQ:QueueName"];
            channel.QueueDeclare(
                queue: queueName,
                durable: true,         
                exclusive: false,      
                autoDelete: false,     
                arguments: null        
            );
            var headers = new Dictionary<string, object> { { "TaskType", "Ping" }, { "x-match", "any" } };

            channel.QueueBind(queueName,exchangename,string.Empty,headers);

            var maxConcurrency = 1000;
            var semaphore = new SemaphoreSlim(maxConcurrency);

            var consumer=new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body=ea.Body.ToArray();

                var message = Encoding.UTF8.GetString(body);
                var ipAddresses = JsonConvert.DeserializeObject<List<IPAdresses>>(message);
                var pingTasks = new List<Task>();
                var PingResList=new List<PingRecord>();

                foreach (var ip in ipAddresses)
                {
                    await semaphore.WaitAsync(); 

                    var pingTask = Task.Run(async () =>
                    {
                        try
                        {
                            var pingResult = await _pingHelper.Pinger(ip.IPAddress);

                            var pingRecord = new PingRecord
                            {
                                IPAddress = ip.IPAddress,
                                Status = pingResult.Status.ToString(),
                                Rtt = pingResult.RoundtripTime>0 ? pingResult.RoundtripTime:-1,
                                Time=DateTime.Now.ToUniversalTime()
                            };

                            PingResList.Add(pingRecord);

                            Console.WriteLine($"Processed IP: {pingRecord.IPAddress}, Status: {pingRecord.Status}, RTT: {pingRecord.Rtt}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing IP: {ip.IPAddress}. Exception: {ex.Message}");
                        }
                        finally
                        {
                            semaphore.Release(); 
                        }
                    });

                    pingTasks.Add(pingTask);
                }

              
                await Task.WhenAll(pingTasks);
                var resultMessage = JsonConvert.SerializeObject(PingResList);
                Stopwatch sw= Stopwatch.StartNew();
               
               var rows=await _databaseService.InsertRecordsAsync(resultMessage);
                sw.Stop();

                Console.WriteLine($"{rows} rows inserted Succesfully in {sw.ElapsedMilliseconds} time");
                Console.WriteLine("Batch processing completed.");
            };


            channel.BasicConsume(queue: queueName, autoAck: true, consumer:consumer);

            Console.WriteLine("Ping Consumer started listening.");
            Task.Delay(Timeout.Infinite).Wait();

        }

        }
    }
