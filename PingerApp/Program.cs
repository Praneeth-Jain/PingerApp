using Microsoft.Extensions.Configuration;
using PingerApp.Configuration;
using PingerApp.Services;
using Microsoft.Extensions.DependencyInjection;
using PingerApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;



public class PingMain
{
    public static async Task Main(string[] args)
    {
        var ServiceProvider = ConfigureServices();
        try
        {

            var ConsumerService = ServiceProvider.GetRequiredService<IPingConsumerService>();
            var ProducerService = ServiceProvider.GetRequiredService<IPingProducerService>();
            //var DBService = ServiceProvider.GetRequiredService<IDatabaseConsumerService>();

            var producerTask = Task.Run(async () => await ProducerService.PublishIPAddressesAsync());
            var consumerTask = Task.Run(() => ConsumerService.StartListening());

            await Task.WhenAll(producerTask, consumerTask);
            //DBService.StartConsumer();


        }
        catch (Exception ex)
        {
            Console.WriteLine($"There was some error in the application :"+ex.Message);
        }
        finally
        {
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
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
        services.AddLogging(config => config.AddConsole()); 
        services.AddSingleton<IRabbitMQHelper, RabbitMQHelper>();
        services.AddScoped<IPingProducerService, PingProducerService>();
        services.AddScoped<IPingConsumerService,PingConsumerService>();
        services.AddScoped<IDatabaseConsumerService, DatabaseConsumerService>();
        services.AddScoped<IDatabaseService, DatabaseService>();
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