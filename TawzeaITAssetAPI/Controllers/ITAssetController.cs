using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TawzeaITAssetAPI.Data;
using TawzeaITAssetAPI.Models;

namespace TawzeaITAssetAPI.Controllers;

// All CRUD operations go through dbo.sp_ITAsset_* stored procedures
// (created in the AddITAssetStoredProcedures migration)
[ApiController]
public class ITAssetController : ControllerBase
{
    private readonly ILogger<ITAssetController> _logger;
    private readonly AppDbContext _db;

    public ITAssetController(ILogger<ITAssetController> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    [HttpGet("/ITAsset/List")]
    public async Task<ActionResult<List<ITAsset>>> List()
        => await ListAllAsync();

    [HttpGet("/ITAsset/getById/{id:guid}")]
    public async Task<ActionResult<ITAsset>> GetById(Guid id)
    {
        var asset = (await _db.ITAssets
                .FromSql($"EXEC dbo.sp_ITAsset_GetById @Id = {id}")
                .AsNoTracking()
                .ToListAsync())
            .FirstOrDefault();

        return asset is null ? NotFound(new { message = "Asset not found" }) : asset;
    }

    [HttpPost("/ITAsset/create")]
    public async Task<ActionResult<List<ITAsset>>> Create(ITAssetRequest req)
    {
        try
        {
            await _db.Database.ExecuteSqlAsync($@"EXEC dbo.sp_ITAsset_Create
                @Brand = {req.Brand},
                @SerialNumber = {req.SerialNumber},
                @WarrantyExpirationDate = {req.WarrantyExpirationDate},
                @Status = {req.Status!.Value.ToString()}");
        }
        catch (DbException ex) when (IsDuplicateKey(ex))
        {
            return Conflict(new { message = "Serial number already exists" });
        }

        return await ListAllAsync();
    }

    [HttpPut("/ITAsset/update/{id:guid}")]
    public async Task<ActionResult<List<ITAsset>>> Update(Guid id, ITAssetRequest req)
    {
        int rows;
        try
        {
            rows = await ExecRowCountAsync($@"EXEC dbo.sp_ITAsset_Update
                @Id = {id},
                @Brand = {req.Brand},
                @SerialNumber = {req.SerialNumber},
                @WarrantyExpirationDate = {req.WarrantyExpirationDate},
                @Status = {req.Status!.Value.ToString()}");
        }
        catch (DbException ex) when (IsDuplicateKey(ex))
        {
            return Conflict(new { message = "Serial number already exists" });
        }

        if (rows == 0) return NotFound(new { message = "Asset not found" });
        return await ListAllAsync();
    }

    [HttpDelete("/ITAsset/delete/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var rows = await ExecRowCountAsync($"EXEC dbo.sp_ITAsset_Delete @Id = {id}");

        return rows == 0
            ? NotFound(new { message = "Asset not found" })
            : Ok(new { message = "Asset deleted successfully" });
    }

    // ---- helpers ----

    private async Task<List<ITAsset>> ListAllAsync()
        => await _db.ITAssets
            .FromSql($"EXEC dbo.sp_ITAsset_List")
            .AsNoTracking()
            .ToListAsync();

    // Relies on the SP leaving NOCOUNT off, so ExecuteNonQuery reports the affected rows
    private async Task<int> ExecRowCountAsync(FormattableString sql)
        => await _db.Database.ExecuteSqlAsync(sql);

    private bool IsDuplicateKey(Exception ex)
    {
        var sqlEx = ex as SqlException ?? ex.InnerException as SqlException;
        var isDup = sqlEx is { Number: 2627 or 2601 };
        if (isDup) _logger.LogWarning(sqlEx, "Duplicate serial number");
        return isDup;
    }
}