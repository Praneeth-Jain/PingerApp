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
        public static PingReply Pinger(string address)
        {
            Ping ping = new Ping();
            PingReply reply = null!;
            reply =ping.Send(address, 3000);
            return reply;
        }
    }
}

