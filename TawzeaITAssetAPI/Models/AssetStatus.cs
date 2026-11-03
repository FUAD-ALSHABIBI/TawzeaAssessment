using System.ComponentModel.DataAnnotations;

namespace TawzeaITAssetAPI.Models
{
    public enum AssetStatus
    {
        New = 1,
        [Display(Name = "In Use")]
        InUse = 2,
        Damaged = 3,
        Disposed = 4
    }
}