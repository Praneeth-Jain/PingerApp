using Microsoft.Extensions.Configuration;

namespace PingerApp.Configuration
{
    public interface IConfigurationHelper
    {
        string GetFilePathString(string name);
    }

    public class ConfigurationHelper : IConfigurationHelper
    {
        private readonly IConfiguration _configuration;

        public ConfigurationHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetFilePathString(string name)
        {
            var filePathString = _configuration["FilePaths:" + name];
            if (string.IsNullOrEmpty(filePathString))
            {
                throw new InvalidOperationException($"The file path '{name}' is not defined in the configuration.");
            }
            return filePathString;
        }
    }
}
