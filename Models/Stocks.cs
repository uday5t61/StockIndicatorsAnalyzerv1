namespace StockIndicatorsAnalyzer.Models
{
    public class Stocks
    {
        public string Symbol { get; set; }
        public List<Stock> StockInfo { get; set; } = new List<Stock>();
    }
}
