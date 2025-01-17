using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using PingerApp.Services;
using PingerApp.Configuration;
using PingerApp.Worker;
using PingerApp.Model;
using Microsoft.Extensions.Options;
using PingerApp.Helpers.RabbitMQhelpers;
using PingerApp.Helpers.DBHelpers;


namespace PingerApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                await CreateHostBuilder(args).Build().RunAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddUserSecrets<Program>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Information);
                    logging.AddNLog("nlog.config");
                })
                .ConfigureServices((hostContext, services) =>
                {

                services.Configure<RabbitMQSettings>(hostContext.
                    Configuration.GetSection("RabbitMQ"));
                services.AddSingleton(Resolver => Resolver.GetRequiredService<IOptions<RabbitMQSettings>>().Value);
                
                    services.Configure<PingSettings>(hostContext.
                    Configuration.GetSection("PingSettings"));
                services.AddSingleton(Resolver => Resolver.GetRequiredService<IOptions<PingSettings>>().Value);
                    
                    services.AddSingleton<IConfiguration>(hostContext.Configuration);
                    services.AddSingleton<IConfigurationHelper, ConfigurationHelper>();
                    services.AddSingleton<IRabbitMQConnectionManager, RabbitMQConnectionManager>();
                    services.AddScoped<IPingProducerService, PingProducerService>();
                    services.AddScoped<IPingConsumerService, PingConsumerService>();
                    services.AddScoped<IDatabaseService, DatabaseService>();
                    services.AddScoped<IPingHelper, PingHelper>();
                    services.AddSingleton<IRabbitMQHelper, RabbitMQHelper>();
                    services.AddSingleton <DBHelper>();

                    //services.AddScoped<IDatabaseConsumerService, DatabaseConsumerService>();
                    //services.AddScoped<ICsvHelpers, CsvHelpers>();
                    //services.AddDbContext<ApplicationDbContext>(options =>
                    //{
                    //    options.UseNpgsql(hostContext.Configuration.GetConnectionString("DefaultConnection"));
                    //});

                    //services.AddDbContextFactory<ApplicationDbContext>(options =>
                    //{
                    //    options.UseNpgsql(hostContext.Configuration.GetConnectionString("DefaultConnection"));
                    //});

                    services.AddHostedService<PingWorkerService>();
                });
    }
}
