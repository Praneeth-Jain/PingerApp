using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Npgsql;
using PingerApp.Data;
using System.Data;
using System.Threading.Tasks;

namespace PingerApp.Services
{
    public interface IDatabaseService
    {
        Task<int> InsertRecordsAsync(string jsonInput);
    }

    public class DatabaseService : IDatabaseService
    {
        private readonly ApplicationDbContext _context;


        public DatabaseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> InsertRecordsAsync(string jsonInput)
        {
           
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                //command.CommandText = "SELECT Insert_Ping_Records(@ping_results)";
                //command.CommandType = System.Data.CommandType.Text;

                command.CommandText = "INSERT_PING_RECORDS_PROCEDURE";
                command.CommandType = System.Data.CommandType.StoredProcedure;


                var jsonParameter = new NpgsqlParameter("@ping_results", NpgsqlTypes.NpgsqlDbType.Jsonb)
                {
                    Value = jsonInput
                };
                command.Parameters.Add(jsonParameter);

                var outputParam = new NpgsqlParameter("@inserted_count", DbType.Int32)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(outputParam);

                if (command.Connection.State != System.Data.ConnectionState.Open)
                {
                    await command.Connection.OpenAsync();
                }

                //var totalRows=(int)await command.ExecuteScalarAsync();

                await command.ExecuteNonQueryAsync();
                var insertedCount = (outputParam.Value != DBNull.Value) ? (int)outputParam.Value : 0;
                Console.WriteLine($"Function executed successfully");
                return insertedCount;
                //return totalRows;
                
            }
        }
    }
}
