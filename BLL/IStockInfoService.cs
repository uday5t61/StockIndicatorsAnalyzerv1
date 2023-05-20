using StockIndicatorsAnalyzer.Models;

namespace StockIndicatorsAnalyzer.BLL
{
    public interface IStockInfoService
    {
        public Task GetDailyFileFromNSEAsync(DateTime StartDate,DateTime EndDate, int delayBetweenRequests);
        public void UnZipFiles();
        public Task SaveDataToMasterJsonAsync();
        public List<string> GetDownloadedFilesStatus();
        public Task<IEnumerable<Indicators>> GetTechnicalIndicatorsAsync(Request req);
    }
}
