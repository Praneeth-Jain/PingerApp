using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;

namespace PingerApp.Helpers.DBHelpers
{
    public class DBHelper
    {
        public static NpgsqlParameter SafeNpgsqlParameter(string parameterName, NpgsqlDbType data)
        {

            return new NpgsqlParameter(parameterName, data);


        }
        public static NpgsqlParameter SafeNpgsqlParameter(string parameterName, object data)
        {
            if (data != null && data.GetType().IsValueType) return new NpgsqlParameter(parameterName, data);
            if (data == null) return new NpgsqlParameter(parameterName, DBNull.Value);
            return new NpgsqlParameter(parameterName, data);


        }
    }
}
