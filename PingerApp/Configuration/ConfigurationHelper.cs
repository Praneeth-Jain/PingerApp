using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace PingerApp.Configuration
{

    public static class ConfigurationHelper
    {
        private static readonly IConfiguration _configuration;

        static ConfigurationHelper()
        {
            Console.WriteLine(AppContext.BaseDirectory);
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();
        }

        public static string GetFilePathString(string name)
        {
           

            var FilePathString = _configuration["Filepaths:"+name];
            if (string.IsNullOrEmpty(FilePathString))
            {
                throw new InvalidOperationException($"The connection string '{name}' has not been initialized.");
            }
            return FilePathString;
            
        }
    }
}
