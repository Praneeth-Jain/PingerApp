using PingerApp.Model;

namespace PingerApp.Services
{
    public interface ICsvHelpers
    {
        Task<List<string>> ReadCsv(string filePath);

        void WriteToCsv(string filePath, List<FileModel> Info);
    }
}
