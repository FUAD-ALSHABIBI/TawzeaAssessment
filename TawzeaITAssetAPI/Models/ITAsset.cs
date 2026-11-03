

using System.ComponentModel.DataAnnotations;

namespace TawzeaITAssetAPI.Models
{
    public class ITAsset
    {

        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public string Brand { get; set; }

        [Required, MaxLength(100)]
        public string SerialNumber { get; set; }

        public DateTime WarrantyExpirationDate { get; set; }

        public AssetStatus Status { get; set; } = AssetStatus.New;

        public DateTime CreatedDate { get; set; }
    }
}