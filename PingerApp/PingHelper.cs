using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace PingerApp
{
    public class PingHelper
    {
        public static async Task<PingReply> Pinger(string address)
        {

            Ping ping = new Ping();
            PingReply reply = null!;
            int count = 0;
            while (count<3)
            {
            reply =await ping.SendPingAsync(address, 2000);
                if (reply.Status.ToString() == "Success") { return reply; }
            count++;
            }
            return reply;
        }
    }
}

