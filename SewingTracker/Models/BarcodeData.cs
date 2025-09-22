namespace SewingTracker.Models
{
    public class BarcodeData
    {
        public string Type { get; set; } // "EMPLOYEE" or "RECEIPT"
        public string Data { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
