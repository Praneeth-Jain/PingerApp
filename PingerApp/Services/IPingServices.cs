using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PingerApp.Data.Entity;

namespace PingerApp.Services
{
    public interface IPingService
    {
        Task PingTaskAsync();

        Task<List<IPAdresses>> GetIPAddressesAsync();
    }
}
