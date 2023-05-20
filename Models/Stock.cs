namespace StockIndicatorsAnalyzer.Models
{
    public class Stock
    {
        public string SYMBOL { get; set; }
        public string SERIES { get; set; }
        public double OPEN { get; set; }
        public double HIGH { get; set; }
        public double LOW { get; set; }
        public double CLOSE { get; set; }
        public double LAST { get; set; }
        public double PREVCLOSE { get; set; }
        public double TOTTRDQTY { get; set; }
        public double TOTTRDVAL { get; set; }
        public DateTime TIMESTAMP { get; set; }
        public int TOTALTRADES { get; set; }
        public string ISIN { get; set; }
    }
}
