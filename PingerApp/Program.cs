using PingerApp;

public class PingMain
{
    public static void Main(string[] args)
    {
        CsvHelpers ch = new CsvHelpers();
        var list=ch.ReadCsv("C:\\Users\\PRANEET JAIN\\Downloads\\msft-public-ips.csv");
        foreach( var item in list)
        {
            var res = PingHelper.Pinger(item);
            var fileWrite = new FileModel
            {
                Address = res.Address.ToString(),
                Status = res.Status.ToString(),
                Rtt = res.RoundtripTime,
                Time = DateTime.Now
            };
            ch.WriteToCsv("D:\\ping\\result.csv", fileWrite);
        }
    }
}