using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PingerApp.Data;
using Newtonsoft.Json;

namespace PingerApp.Services
{
    public interface IPingProducerService
    {
        Task PublishIPAddressesAsync();
    }
    public class PingProducerService : IPingProducerService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly IRabbitMQHelper _rabbitMQHelper;
        public PingProducerService(IConfiguration configuration, ApplicationDbContext context, IRabbitMQHelper rabbitMQHelper)
        {
            _configuration = configuration;
            _context = context;
            _rabbitMQHelper = rabbitMQHelper;
        }

        public async Task PublishIPAddressesAsync()
        {
            var IpAddresses = await _context.IPadresses.ToListAsync();
            var ExchangeName = _configuration["RabbitMQ:ExchangeName"];
           
                var headers = new Dictionary<string, object> { { "TaskType", "Ping" } };
                var message = JsonConvert.SerializeObject(IpAddresses);
                try
                {
                    _rabbitMQHelper.PublishMessage(ExchangeName, string.Empty, headers, message);
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); };
            
            Console.WriteLine("All IPs successfully published to RabbitMQ");
        }
    }
}
