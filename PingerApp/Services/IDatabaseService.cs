using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using PingerApp.Data;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;
    
namespace PingerApp.Services
{
    public interface IDatabaseService
    {
        Task<int> InsertRecordsAsync(string jsonInput);
    }

    public class DatabaseService : IDatabaseService
    {
        private string connectionString;

        private readonly ILogger<IDatabaseService> _logger;

        public DatabaseService(IConfiguration configuration,ILogger<IDatabaseService> logger)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger=logger;
        }

        public async Task<int> InsertRecordsAsync(string jsonInput)
        {
            try
            {


            using (var command = new NpgsqlConnection(connectionString).CreateCommand()){ 
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
                _logger.LogInformation($"Stored Procedure executed successfully");
                    //return totalRows;
                   
                    return  (outputParam.Value != DBNull.Value) ? (int)outputParam.Value : 0;
                   
            }
                
            }catch (Exception ex) { _logger.LogError($"An error occured while executing Stored procedure {ex.Message}");
                throw;
            };
        }
    }
}
