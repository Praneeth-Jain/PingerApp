using Microsoft.Extensions.Configuration;
using PingerApp.Configuration;
using PingerApp.Services;
using Microsoft.Extensions.DependencyInjection;
using PingerApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


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

            ConsumerService.StartListening();
            await ProducerService.PublishIPAddressesAsync();
            DBService.StartConsumer();

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
        services.AddScoped<IPingService, PingService>();
        services.AddScoped<ICsvHelpers, CsvHelpers>();
        services.AddScoped<IPingHelper, PingHelper>();
        services.AddLogging(config => config.AddConsole()); // Add logging support here
        services.AddSingleton<IRabbitMQHelper, RabbitMQHelper>();
        services.AddScoped<IPingProducerService, PingProducerService>();
        services.AddScoped<IPingConsumerService,PingConsumerService>();
        services.AddScoped<IDatabaseConsumerService, DatabaseConsumerService>();
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });
        services.AddDbContextFactory<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        return services.BuildServiceProvider();
    }
}