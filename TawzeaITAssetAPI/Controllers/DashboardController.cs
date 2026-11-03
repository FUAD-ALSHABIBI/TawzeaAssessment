using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TawzeaITAssetAPI.Data;
using TawzeaITAssetAPI.Models;

namespace TawzeaITAssetAPI.Controllers;

// All reads go through dbo.sp_Dashboard_* stored procedures
// (created in the AddDashboardStoredProcedures migration)
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("/Dashboard")]
    public async Task<IActionResult> GetData()
    {
        var assetsByBrand = await _db.Database
            .SqlQuery<BrandCount>($"EXEC dbo.sp_Dashboard_AssetsByBrand")
            .ToListAsync();

        var assetsByStatus = await _db.Database
            .SqlQuery<StatusCount>($"EXEC dbo.sp_Dashboard_AssetsByStatus")
            .ToListAsync();

        var warrantyExpiringThisMonth = await _db.ITAssets
            .FromSql($"EXEC dbo.sp_Dashboard_WarrantyExpiringThisMonth")
            .AsNoTracking()
            .ToListAsync();

        return Ok(new
        {
            // Every asset has exactly one status, so this equals the row count
            totalAssets = assetsByStatus.Sum(s => s.Count),
            assetsByBrand,
            assetsByStatus,
            warrantyExpiringThisMonth
        });
    }
}
