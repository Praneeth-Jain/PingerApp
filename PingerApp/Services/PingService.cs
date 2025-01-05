using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PingerApp.Configuration;
using PingerApp.Model;

namespace PingerApp.Services
{
    public interface IPingService
    {
        Task PingTaskAsync();
    }
    public class PingService:IPingService
    {
        private readonly IPingHelper _pingHelper;
        private readonly ICsvHelpers _csvHelpers;
        private readonly IConfiguration _configuration;

        public PingService(IPingHelper pingHelper, ICsvHelpers csvHelper, IConfiguration configuration)
        {
            _pingHelper = pingHelper;
            _csvHelpers = csvHelper;
            _configuration = configuration;
        }

        public async Task PingTaskAsync() {
            try
            {
                var inputPath = _configuration["FilePaths:InputPath"];
                var outputPath = _configuration["FilePaths:OutputPath"];

                if (string.IsNullOrEmpty(inputPath) || string.IsNullOrEmpty(outputPath))
                {
                    throw new InvalidOperationException("Input or Output path is not defined in the configuration.");
                }
                var list = await _csvHelpers.ReadCsv(inputPath);

                var maxLimit = 500;
                SemaphoreSlim semaphore = new SemaphoreSlim(maxLimit);
                var csvList = new List<FileModel>();
                var PingTaskList = new List<Task>();
                foreach (var item in list)
                {
                    await semaphore.WaitAsync();

                    var pingTask = Task.Run(async () =>
                    {

                        try
                        {

                            var res = await _pingHelper.Pinger(item);
                            var csvItems = new FileModel
                            {
                                Address = item,
                                Status = res.Status.ToString(),
                                Rtt = res.Status.ToString() == "Success" ? res.RoundtripTime : -1,
                                Time = DateTime.Now
                            };
                            csvList.Add(csvItems);
                            Console.WriteLine("Done");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        finally { semaphore.Release(); }
                    });
                    PingTaskList.Add(pingTask);
                };
                await Task.WhenAll(PingTaskList);
                _csvHelpers.WriteToCsv(outputPath, csvList);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
