namespace TawzeaITAssetAPI.Models
{
    // Row shapes returned by the sp_Dashboard_* stored procedures
    public class BrandCount
    {
        public string Brand { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class StatusCount
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
