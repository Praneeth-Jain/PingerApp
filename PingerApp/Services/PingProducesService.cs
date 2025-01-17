using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Npgsql;
using PingerApp.Data.Entity;
using PingerApp.Helpers.RabbitMQhelpers;
using PingerApp.Model;

namespace PingerApp.Services
{
    public interface IPingProducerService
    {
        Task PublishIPAddressesAsync();
    }
    public class PingProducerService : IPingProducerService
    {
        private readonly IConfiguration _configuration;
        private readonly IRabbitMQHelper _rabbitMQHelper;
        private readonly RabbitMQSettings _rabbitMQSettings;
        private string connectionString;
        
        public PingProducerService(IConfiguration configuration, IRabbitMQHelper rabbitMQHelper,RabbitMQSettings rabbitMQSettings)
        {
            _configuration = configuration;
            _rabbitMQHelper = rabbitMQHelper;
            _rabbitMQSettings = rabbitMQSettings;
            connectionString = configuration.GetConnectionString("DefaultConnection")??string.Empty;
        }

        public async Task PublishIPAddressesAsync()
        {
            var records = new List<IPAdresses>();
            try
            {

            using(var connection = new NpgsqlConnection(connectionString))
{
                await connection.OpenAsync();

                var fetchQuery = "SELECT * FROM ipaddresses";
                using (var command = new NpgsqlCommand(fetchQuery, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    
                    while (await reader.ReadAsync())
                    {
                        var record = new IPAdresses
                        {
                            IPAddress=reader.GetString(0),
                        };
                        records.Add(record);
                    }
                }
            }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }




            var ExchangeName = _rabbitMQSettings.ExchangeName;
           
                    var headers = new Dictionary<string, object> { { "TaskType", "Ping" } };
                var message = JsonConvert.SerializeObject(records);
                try
                {
                    _rabbitMQHelper.PublishMessage(ExchangeName, string.Empty, headers, message);
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); };
            
            Console.WriteLine("All IPs successfully published to RabbitMQ");
        }
    }
}
