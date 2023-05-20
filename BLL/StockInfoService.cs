using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using NetHostedService.BackGroundTaskQueue;
using Newtonsoft.Json;
using StockIndicatorsAnalyzer.CacheProvider;
using StockIndicatorsAnalyzer.Models;
using StockIndicatorsAnalyzer.Utilities;
using System.IO.Pipelines;
using System.Linq;

namespace StockIndicatorsAnalyzer.BLL
{
    public class StockInfoService : IStockInfoService
    {
       // private static IStockInformationService _stockInformationService;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly ILogger _logger;
        private readonly IMemoryCache _cache;

        //private static IConfiguration _configuration;
        private static string destFilePath;
        private static string zipSourcePath;
        private static string zipDestPath;
        private static string masterJsonDestPath;

        private const string ZIP_STORE_DESTPATH = "zip_store_destpath";
        private const string ZIP_SOURCE_PATH = "zip_source_path";
        private const string ZIP_DESTPATH = "unzip_dest_path";
        private const string MASTER_JSON_DEST_PATH = "master_json_dest_path";

        private const string STOCK_DATA_CACHEKEY = "masterJsonData";

        private static readonly SemaphoreSlim semaphore = new(1, 1);

        public StockInfoService(IConfiguration configuration,
            IBackgroundTaskQueue backgroundTaskQueue, ILogger<StockInfoService> logger, IMemoryCache cache)
        {
            _taskQueue = backgroundTaskQueue;
            destFilePath = configuration.GetValue<string>(ZIP_STORE_DESTPATH);
            zipSourcePath = configuration.GetValue<string>(ZIP_SOURCE_PATH);
            zipDestPath = configuration.GetValue<string>(ZIP_DESTPATH);
            masterJsonDestPath = configuration.GetValue<string>(MASTER_JSON_DEST_PATH);
            _logger = logger;
            _cache = cache;
        }
        public async Task GetDailyFileFromNSEAsync(DateTime StartDate, DateTime EndDate, int delayBetweenRequests)
        {
            //Running downloading of the file in background and sending response to api
            await _taskQueue.QueueBackgroundWorkItemAsync((ct) => BuildWorkItem(StartDate, EndDate, delayBetweenRequests, ct));
        }

        public void UnZipFiles()
        {
            try
            {
                DirectoryInfo d = new DirectoryInfo(zipSourcePath); //Assuming Test is your Folder

                FileInfo[] Files = d.GetFiles("*.zip"); //Getting Text filesList

                foreach (FileInfo file in Files)
                {
                    if (file.Length > 0)
                    {
                        FileIOUtility.UnZipFiles(@$"{zipSourcePath}\{file.Name}", zipDestPath, true);
                    }
                }
            }
            catch
            {
                throw;
            }
        }
        public List<string> GetDownloadedFilesStatus()
        {
            try
            {
                List<string> filesList = new();
                DirectoryInfo d = new DirectoryInfo(zipSourcePath); //Assuming Test is your Folder

                FileInfo[] Files = d.GetFiles("*.zip"); //Getting Text filesList

                foreach (FileInfo file in Files)
                {
                    filesList.Add($"{file.Name}-Size-{file.Length}");
                }
                return filesList;
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<Indicators>> GetTechnicalIndicatorsAsync(Request req)
        {
            //var stock = (await GetStocksData()).FirstOrDefault();
            //var closePrices = stock.StockInfo.Select(s => s.CLOSE).ToArray();
            //var timeStamps = stock.StockInfo.Select(s => s.TIMESTAMP).ToArray();
            //var re = IndicatorsCalculator.CalculateAllIndicators(stock.Symbol,closePrices, timeStamps, 14, 0, stock.StockInfo.Count - 1);
            //return new List<Indicators> { re };
            var stocks = await GetStocksData();
            List<Indicators> indicators = new();
            var from = req?.From ?? 0;
            var take = req?.To ?? stocks.Count - from;
            stocks?.Skip(from).Take(take).ToList().ForEach(stock =>
            {
                var closePrices = stock.StockInfo.Select(s => s.CLOSE).ToArray();
                var timeStamps = stock.StockInfo.Select(s => s.TIMESTAMP).ToArray();
                var indicator = IndicatorsCalculator.CalculateAllIndicators(stock.Symbol, closePrices, timeStamps, 14, 0, stock.StockInfo.Count - 1);
                indicators.Add(indicator);
                
            });
            return indicators;
            //return from stock in stocks
            //       let closePrices = stock.StockInfo.Select(s => s.CLOSE).ToArray()
            //       let timeStamps = stock.StockInfo.Select(s => s.TIMESTAMP).ToArray()
            //       select IndicatorsCalculator.CalculateAllIndicators(stock.Symbol,closePrices, timeStamps, 14, 0, stock.StockInfo.Count-1);
        }

        private async Task<List<Stocks>> GetStocksData()
        {
            _logger.Log(LogLevel.Information, "Trying to fetch the list of employees from cache.");
            if (_cache.TryGetValue(STOCK_DATA_CACHEKEY, out List<Stock>? cachedData))
            {
                _logger.Log(LogLevel.Information, "Data found in cache.");
            }
            else
            {
                try
                {
                    await semaphore.WaitAsync();
                    if (_cache.TryGetValue(STOCK_DATA_CACHEKEY, out cachedData))
                    {
                        _logger.Log(LogLevel.Information, "Data found in cache.");
                    }
                    else
                    {
                        _logger.Log(LogLevel.Information, "Data not found in cache. Fetching from file.");

                        var dataAsString = await File.ReadAllTextAsync(masterJsonDestPath); // csv file location
                        cachedData = JsonConvert.DeserializeObject<List<Stock>>(dataAsString);

                        var cacheEntryOptions = new MemoryCacheEntryOptions()
                                .SetAbsoluteExpiration(TimeSpan.FromDays(1))
                                .SetSize(1);

                        _cache.Set(STOCK_DATA_CACHEKEY, cachedData, cacheEntryOptions);
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            }

            var stocksGroupBySymbol = cachedData.GroupBy(
                s => s.SYMBOL,
                s => s,
                (key, group) => new Stocks{ Symbol = key, StockInfo = group.ToList() }).ToList();

            return stocksGroupBySymbol;
        }

        public async Task SaveDataToMasterJsonAsync()
        {
            try
            {
                DirectoryInfo d = new DirectoryInfo(zipDestPath);

                FileInfo[] Files = d.GetFiles("*.csv"); //Getting csv filesList

                //StocksInMultipleDays list = new StocksInMultipleDays
                //{
                //    StocksAcrorssDays = new List<Stocks>()
                //};
                var stocks = new List<Stock>();

                foreach (FileInfo file in Files)
                {
                    var stocksInaDayAsString = await FileIOUtility.CsvToJsonStringAsync(@$"{zipDestPath}\{file.Name}");
                    var stocksInaDay = JsonConvert.DeserializeObject<List<Stock>>(stocksInaDayAsString);
                    stocks.AddRange(stocksInaDay);
                    //var stocks = new Stocks
                    //{
                    //    StocksList = stocksInaDay
                    //};
                    //list.StocksAcrorssDays.Add(stocks);
                }
                var jsonData = JsonConvert.SerializeObject(stocks);
                await FileIOUtility.WriteToFileAsync(jsonData, masterJsonDestPath);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async ValueTask BuildWorkItem(DateTime StartDate, DateTime EndDate, int delayBetweenRequests, CancellationToken token)
        {
            // Simulate three 5-second tasks to complete
            // for each enqueued work item

            int delayLoop = 0;
            var guid = Guid.NewGuid().ToString();

            _logger.LogInformation("Queued Background Task {Guid} is starting.", guid);

            while (!token.IsCancellationRequested && delayLoop < 1)
            {
                try
                {
                   // await _stockInformationService.GetDailyFileFromNSEAsync(destFilePath, StartDate, EndDate, delayBetweenRequests);                 
                }
                catch (OperationCanceledException)
                {
                    // Prevent throwing if the Delay is cancelled
                }

                delayLoop++;

                _logger.LogInformation("Queued Background Task {Guid} is running. "
                                       + "{DelayLoop}/1", guid, delayLoop);
            }

            if (delayLoop == 1)
            {
                _logger.LogInformation("Queued Background Task {Guid} is complete.", guid);
            }
            else
            {
                _logger.LogInformation("Queued Background Task {Guid} was cancelled.", guid);
            }
        }
    }
}
