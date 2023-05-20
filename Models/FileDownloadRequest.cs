namespace StockIndicatorsAnalyzer.Models
{
    public class FileDownloadRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        /// <summary>
        /// In MilliSeconds
        /// </summary>
        public int DelayBetweenRequests { get; set; }
    }
}
