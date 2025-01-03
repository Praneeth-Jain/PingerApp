using PingerApp;

public class PingMain
{
    public static async Task Main(string[] args)
    {
        CsvHelpers ch = new CsvHelpers();
        var list=await ch.ReadCsv("C:\\Users\\PRANEET JAIN\\Downloads\\msft-public-ips.csv");
        var PingTask = list.Select(async item =>
        {
            try
            {

                var res = await PingHelper.Pinger(item);
                var fileWrite = new FileModel
                {
                    Address = item,
                    Status = res.Status.ToString(),
                    Rtt = res.Status.ToString()=="Success"?res.RoundtripTime:-1,
                    Time = DateTime.Now
                };
                await ch.WriteToCsv("C:\\Users\\PRANEET JAIN\\source\\repos\\PingerApp\\PingerApp\\Output.csv", fileWrite);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        });
        try
        {
            await Task.WhenAll(PingTask);
        }
        catch (Exception ex) { Console.WriteLine(ex.Message); }
        finally
        {
            Console.WriteLine("Ping Task is Completed");
        }
    }
}