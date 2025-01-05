using Microsoft.Extensions.Configuration;
using PingerApp;
using PingerApp.Configuration;
using PingerApp.Helpers;
using PingerApp.Model;

public class PingMain
{
    public static async Task Main(string[] args)
    {
        try
        {
            CsvHelpers ch = new CsvHelpers();
            var inputString = ConfigurationHelper.GetFilePathString("InputPath");
            var outputString = ConfigurationHelper.GetFilePathString("OutputPath");
            var list=await ch.ReadCsv(inputString);

            var maxLimit = 500;
            SemaphoreSlim semaphore = new SemaphoreSlim(maxLimit); 
            var csvList=new List<FileModel >();
            var PingTaskList=new List<Task>();
            foreach (var item in list)
            {
                await semaphore.WaitAsync();

                var pingTask = Task.Run(async() =>
                {

                    try
                    {

                        var res = await PingHelper.Pinger(item);
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
                    catch (Exception ex) {
                        Console.WriteLine(ex.Message);
                    }
                    finally { semaphore.Release(); }
                });
                PingTaskList.Add(pingTask);
            };
                await Task.WhenAll(PingTaskList);
                ch.WriteToCsv(outputString, csvList);
                }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
    }
}