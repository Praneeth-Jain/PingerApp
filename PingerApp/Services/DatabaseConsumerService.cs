using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PingerApp.Data;
using PingerApp.Model;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

public interface IDatabaseConsumerService
{
    void StartConsumer();

    Task ProcessMessageAsync(string message);

    Task SaveBatchToDatabaseAsync();

    Task FlushRemainingRecordsAsync();

    PingRecord DeserializeMessage(string message);

}
public class DatabaseConsumerService:IDatabaseConsumerService
{
    private readonly ILogger<DatabaseConsumerService> _logger;
    private readonly IConfiguration _configuration;
    private readonly SemaphoreSlim _semaphore;
    private readonly int _batchSize;
    private readonly List<PingRecord> _pingRecords;
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public DatabaseConsumerService(ILogger<DatabaseConsumerService> logger,IConfiguration configuration, IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _semaphore = new SemaphoreSlim(500);
        _batchSize = 1000;  
        _pingRecords = new List<PingRecord>();
        _dbContextFactory = dbContextFactory;
    }


    public void StartConsumer()
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

        var headers = new Dictionary<string, object> { { "TaskType", "StoreResult" }, { "x-match", "any" } };
        channel.QueueBind(queue: queueName, exchange: exchangename, routingKey: string.Empty, arguments: headers);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (sender, e) =>
        {
            var message = Encoding.UTF8.GetString(e.Body.ToArray());
            await ProcessMessageAsync(message);
        };

        channel.BasicConsume(queueName, autoAck: true, consumer);

        Console.WriteLine(" [*] Waiting for messages...");
        Console.ReadLine();
    }

    public async Task ProcessMessageAsync(string message)
    {
        try
        {
            var pingRecord = DeserializeMessage(message);

            await _semaphore.WaitAsync();

            try
            {
                _pingRecords.Add(pingRecord);

                if (_pingRecords.Count >= _batchSize)
                {
                    await SaveBatchToDatabaseAsync();
                }
            }
            catch (Exception ex)    
            {
                _logger.LogError($"Error processing message: {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deserializing message: {ex.Message}");
        }
    }

    public async Task SaveBatchToDatabaseAsync()
    {
        try
        {
            if (_pingRecords.Any())
            {
                using (var dbContext = _dbContextFactory.CreateDbContext())
                {


                    await dbContext.PingRecords.AddRangeAsync(_pingRecords);
                    await dbContext.SaveChangesAsync();

                    _logger.LogInformation($"Successfully saved {_pingRecords.Count} records to the database.");

                    _pingRecords.Clear();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error saving batch to database: {ex.Message}");
        }
    }

    public async Task FlushRemainingRecordsAsync()
    {
        await _semaphore.WaitAsync();

        try
        {
            if (_pingRecords.Any())
            {
                await SaveBatchToDatabaseAsync();
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public PingRecord DeserializeMessage(string message)
    {
        return Newtonsoft.Json.JsonConvert.DeserializeObject<PingRecord>(message);
    }
}
