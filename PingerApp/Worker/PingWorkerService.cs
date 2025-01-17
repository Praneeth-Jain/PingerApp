using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using PingerApp.Services;

namespace PingerApp.Worker
{
    public class PingWorkerService : BackgroundService
    {
        

        private readonly IPingProducerService _pingProducerService;
        private readonly IPingConsumerService _pingConsumerService;

        public PingWorkerService(IPingProducerService pingProducerService, IPingConsumerService pingConsumerService)
        {
            _pingProducerService = pingProducerService;
            _pingConsumerService = pingConsumerService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
         
            var producerTask = Task.Run(async () => await _pingProducerService.PublishIPAddressesAsync(), stoppingToken);
            var consumerTask = Task.Run(() => _pingConsumerService.StartListening(), stoppingToken);

            await Task.WhenAll(producerTask, consumerTask);
            
        }
    }
}
