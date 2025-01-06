using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PingerApp.Data;
using PingerApp.Data.Entity;
using PingerApp.Model;

namespace PingerApp.Services
{
 
    public class PingService:IPingService
    {
        private readonly IPingHelper _pingHelper;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public PingService(IPingHelper pingHelper,IConfiguration configuration,ApplicationDbContext context)
        {
            _pingHelper = pingHelper;
            _configuration = configuration;
            _context = context;
        }

        public async Task PingTaskAsync() {
            try
            {
                //var inputPath = _configuration["FilePaths:InputPath"];
                //var outputPath = _configuration["FilePaths:OutputPath"];

                //if (string.IsNullOrEmpty(inputPath) || string.IsNullOrEmpty(outputPath))
                //{
                //    throw new InvalidOperationException("Input or Output path is not defined in the configuration.");
                //}
                var list = await GetIPAddressesAsync();

                var maxLimit = 2000;
                SemaphoreSlim semaphore = new SemaphoreSlim(maxLimit);
                var csvList = new List<PingRecord>();
                var PingTaskList = new List<Task>();
                foreach (var item in list)
                {
                    await semaphore.WaitAsync();

                    var pingTask = Task.Run(async () =>
                    {

                        try
                        {

                            var res = await _pingHelper.Pinger(item.IPAddress);
                            var csvItems = new PingRecord
                            {
                                IPAddress = item.IPAddress,
                                Status = res.Status.ToString(),
                                Rtt = res.Status.ToString() == "Success" ? res.RoundtripTime : -1,
                                Time = DateTime.Now.ToUniversalTime(),
                            };
                            csvList.Add(csvItems);
                            Console.WriteLine("Done");
                        }
                        catch (Exception ex) {Console.WriteLine("Error While Pinging the Address :"+ex.Message);}
                        finally { semaphore.Release(); }
                    });
                    PingTaskList.Add(pingTask);
                };
                await Task.WhenAll(PingTaskList);
                await _context.PingRecords.AddRangeAsync(csvList);
                await _context.SaveChangesAsync();
                Console.WriteLine("Ping Records Successfully Added into the DB");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured during the PingService "+ex.Message);
            }
        }
        public async Task<List<IPAdresses>> GetIPAddressesAsync()
        {
            try
            {
                return await _context.IPadresses.ToListAsync();
            }
            catch (Exception ex) { Console.WriteLine("Error while Reading Data from Db" + ex.Message); }
            return null;
        }
    }
}
