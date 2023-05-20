namespace StockIndicatorsAnalyzer.Models
{
    public class Indicators
    {
        public string Symbol { get; set; }
        public List<IndicatorData> RSI { get; set; }
    }
}
