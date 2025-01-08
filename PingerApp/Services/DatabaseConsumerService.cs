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
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseConsumerService> _logger;
    private readonly IConfiguration _configuration;
    private readonly SemaphoreSlim _semaphore;
    private readonly int _batchSize;
    private readonly List<PingRecord> _pingRecords;

    public DatabaseConsumerService(ApplicationDbContext context,ILogger<DatabaseConsumerService> logger,IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _semaphore = new SemaphoreSlim(1, 1);
        _batchSize = 1000;  
        _pingRecords = new List<PingRecord>();
    }


    public void StartConsumer()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        var queueName = "pingQueue";
        channel.QueueDeclare(queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

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
                await _context.PingRecords.AddRangeAsync(_pingRecords);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Successfully saved {_pingRecords.Count} records to the database.");

                _pingRecords.Clear();
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
