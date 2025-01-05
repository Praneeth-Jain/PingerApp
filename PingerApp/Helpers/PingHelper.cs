using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PingerApp.Helpers
{
    public class PingHelper
    {
        public static async Task<PingReply> Pinger(string address)
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
                Console.WriteLine(ex.Message);
            }

            return reply;
        }
    }
}

