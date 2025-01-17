using Newtonsoft.Json;
using PingerApp.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PingerApp.Services
{
    public interface IDatabaseService
    {
        Task<int> InsertRecordsAsync(string jsonInput);
    }
}
