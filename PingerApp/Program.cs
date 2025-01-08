using Microsoft.Extensions.Configuration;
using PingerApp.Configuration;
using PingerApp.Services;
using Microsoft.Extensions.DependencyInjection;
using PingerApp.Data;
using Microsoft.EntityFrameworkCore;


public class PingMain
{
    public static async Task Main(string[] args)
    {
        var ServiceProvider = ConfigureServices();
        try
        {

            var ConsumerService = ServiceProvider.GetRequiredService<IPingConsumerService>();
            var ProducerService = ServiceProvider.GetRequiredService<IPingProducerService>();
            var DBService = ServiceProvider.GetRequiredService<IDatabaseConsumerService>();

            // Start the consumer in the background (non-blocking)
            var consumerTask = Task.Run(() => ConsumerService.StartListening());

            // Start producing IP addresses and publishing them to RabbitMQ
            await ProducerService.PublishIPAddressesAsync();

            // Start the database consumer service (to consume from RabbitMQ and store in DB)
            DBService.StartConsumer();

            // Wait for the consumer to finish (if needed), or let it run in the background
            await consumerTask;

        }
        catch (Exception ex)
        {
            Console.WriteLine($"There was some error in the application :"+ex.Message);
        }
    }
    private static ServiceProvider ConfigureServices()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IConfigurationHelper, ConfigurationHelper>();
        //services.AddScoped<IPingService, PingService>();
        //services.AddScoped<ICsvHelpers, CsvHelpers>();
        services.AddScoped<IPingHelper, PingHelper>();
        services.AddSingleton<IRabbitMQHelper, RabbitMQHelper>();
        services.AddScoped<IPingProducerService, PingProducerService>();
        services.AddScoped<IPingConsumerService,PingConsumerService>();
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        return services.BuildServiceProvider();
    }
}