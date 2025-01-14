using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Npgsql;
using PingerApp.Data.Entity;

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
        private string connectionString;
        public PingProducerService(IConfiguration configuration, IRabbitMQHelper rabbitMQHelper)
        {
            _configuration = configuration;
            _rabbitMQHelper = rabbitMQHelper;
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

                var fetchQuery = "SELECT * FROM \"IPadresses\"";
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




            var ExchangeName = _configuration["RabbitMQ:ExchangeName"];
           
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
