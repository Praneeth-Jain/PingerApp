using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;


namespace PingerApp.Services
{
    public class PingHelper:IPingHelper
    {
        private readonly ILogger<IPingHelper> _logger;
        public PingHelper(ILogger<IPingHelper> logger) { _logger = logger; }
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
                    _logger.LogInformation($"The {address} is being pinged {count} times");
                    Console.WriteLine($"The {address} is being pinged {count} times");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occured While Pinging the IP {address} : {ex.Message}");
            }

            return reply;
        }
    }
}

