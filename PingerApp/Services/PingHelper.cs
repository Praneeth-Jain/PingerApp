using System.Net.NetworkInformation;


namespace PingerApp.Services
{
    public class PingHelper:IPingHelper
    {
        public  async Task<PingReply> Pinger(string address)
        {

            Ping ping = new Ping();
            PingReply reply = null!;
            int count = 0;
            try
            {

                while (count < 3)
                {
                    reply = await ping.SendPingAsync(address, 2000);
                    if (reply.Status.ToString() == "Success") { return reply; }
                    count++;
                    Console.WriteLine($"The {address} is being pinged {count} times");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Occured While Pinging the IP {address} : {ex.Message}");
            }

            return reply;
        }
    }
}

