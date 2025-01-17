using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PingerApp.Data.Entity;
using PingerApp.Helpers.RabbitMQhelpers;
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

        private readonly IPingHelper _pingHelper;

        private readonly IRabbitMQHelper _rabbitMqHelper;

        private readonly IDatabaseService _databaseService;

        private readonly ILogger<IPingConsumerService> _logger;
        private readonly RabbitMQSettings _rabbitMQSettings;

        private readonly IRabbitMQConnectionManager _connectionManager;

        private readonly PingSettings _pingSettings;

        public PingConsumerService(IRabbitMQHelper rabbitMqHelper,IPingHelper pingHelper,IDatabaseService databaseService,ILogger<IPingConsumerService> logger,RabbitMQSettings rabbitMQSettings,IRabbitMQConnectionManager rabbitMQConnectionManager,PingSettings pingSettings) 
        {
            _logger = logger;
            _rabbitMQSettings = rabbitMQSettings;
            _connectionManager = rabbitMQConnectionManager;
            _pingSettings = pingSettings;
            _pingHelper = pingHelper;
            _rabbitMqHelper = rabbitMqHelper;
            _databaseService = databaseService;
        }


        public void StartListening()
        {


         
            var channel = _connectionManager.CreateChannel();

            var exchangename = _rabbitMQSettings.ExchangeName;
            channel.ExchangeDeclare(exchange: exchangename, type: ExchangeType.Headers, durable: true, autoDelete: false);

            string queueName = _rabbitMQSettings.QueueName;
            channel.QueueDeclare(
                queue: queueName,
                durable: true,         
                exclusive: false,      
                autoDelete: false,     
                arguments: null        
            );
            var headers = new Dictionary<string, object> { { "TaskType", "Ping" }, { "x-match", "any" } };

            channel.QueueBind(queueName,exchangename,string.Empty,headers);

            var maxConcurrency =_pingSettings.MaxConcurrency;
            var semaphore = new SemaphoreSlim(maxConcurrency);

            var consumer=new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body=ea.Body.ToArray();

                var message = Encoding.UTF8.GetString(body);
                var ipAddresses = JsonConvert.DeserializeObject<IEnumerable<IPAdresses>>(message);
                var pingTasks = new List<Task>();
                var PingResList=new List<PingRecord>();
                Console.WriteLine("Ping Process Started");
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
                                Rtt = pingResult.RoundtripTime > 0 ? pingResult.RoundtripTime : -1,
                                Time=DateTime.Now.ToUniversalTime()
                            };

                            PingResList.Add(pingRecord);

                           _logger.LogInformation($"Processed IP: {pingRecord.IPAddress}, Status: {pingRecord.Status}, RTT: {pingRecord.Rtt}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error processing IP: {ip.IPAddress}. Exception: {ex.Message}");
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

                Console.WriteLine($"{rows} rows inserted Succesfully in {sw.Elapsed.TotalMilliseconds} time");
                _logger.LogInformation("Batch processing completed.");
                
            };


            channel.BasicConsume(queue: queueName, autoAck: true, consumer:consumer);

            Console.WriteLine("Ping Consumer started listening.");
            Task.Delay(Timeout.Infinite).Wait();

        }

        }
    }
