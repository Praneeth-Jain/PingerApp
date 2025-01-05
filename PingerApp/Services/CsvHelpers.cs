using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using PingerApp.Model;


namespace PingerApp.Services
{
    public class CsvHelpers:ICsvHelpers
    {
        private readonly object lockObject = new object();

        public async Task<List<string>> ReadCsv(string filePath)
        {
            var Ip = new List<string>();

            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"The file '{filePath}' was not found.");
                }

                string[] lines = await File.ReadAllLinesAsync(filePath);

                foreach (var line in lines)
                {
                    var values = line.Split(',');


                    if (values.Length > 0)
                    {
                        Ip.Add(values[0].Trim());
                    }
                }

                return Ip;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
            return null;
        }

        public void WriteToCsv(string filePath, List<FileModel> Info)
        {
            lock (lockObject)
            {
                try
                {

                    using (var writer = new StreamWriter(filePath))
                    {
                        foreach (var Ip in Info)
                        {
                            writer.WriteLine($"{Ip.Address},{Ip.Status},{Ip.Rtt},{Ip.Time}");
                        }

                    }

                    Console.WriteLine($"CSV file written successfully to {filePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                }
            }
        }
    }
}
