using System.Net.NetworkInformation;


namespace PingerApp.Services
{
    public interface IPingHelper
    {
        Task<PingReply> Pinger(string address);
    }
}
