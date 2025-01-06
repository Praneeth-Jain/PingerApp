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

            var pingService = ServiceProvider.GetRequiredService<IPingService>();

            await pingService.PingTaskAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
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
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        return services.BuildServiceProvider();
    }
}