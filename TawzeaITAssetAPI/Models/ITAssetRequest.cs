

using System.ComponentModel.DataAnnotations;

namespace TawzeaITAssetAPI.Models
{
   public class ITAssetRequest
    {
        [Required, MaxLength(100)]
        public string Brand { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string SerialNumber { get; set; } = string.Empty;

        [Required]
        public DateTime? WarrantyExpirationDate { get; set; }

        [Required, EnumDataType(typeof(AssetStatus))]
        public AssetStatus? Status { get; set; }
    }
}