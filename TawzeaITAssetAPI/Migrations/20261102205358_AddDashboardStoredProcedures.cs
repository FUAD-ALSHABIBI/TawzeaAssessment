using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TawzeaITAssetAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddDashboardStoredProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Column names match the BrandCount / StatusCount DTOs read by DashboardController
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE [dbo].[sp_Dashboard_AssetsByBrand]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [Brand], COUNT(*) AS [Count]
    FROM [dbo].[ITAssets]
    GROUP BY [Brand]
    ORDER BY [Count] DESC;
END");

            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE [dbo].[sp_Dashboard_AssetsByStatus]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [Status], COUNT(*) AS [Count]
    FROM [dbo].[ITAssets]
    GROUP BY [Status];
END");

            // Current calendar month in DB server time, the same clock as CreatedDate
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE [dbo].[sp_Dashboard_WarrantyExpiringThisMonth]
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @MonthStart datetime2 = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);
    DECLARE @NextMonthStart datetime2 = DATEADD(MONTH, 1, @MonthStart);

    SELECT [Id], [Brand], [SerialNumber], [WarrantyExpirationDate], [Status], [CreatedDate]
    FROM [dbo].[ITAssets]
    WHERE [WarrantyExpirationDate] >= @MonthStart
      AND [WarrantyExpirationDate] < @NextMonthStart
    ORDER BY [WarrantyExpirationDate];
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_Dashboard_WarrantyExpiringThisMonth];");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_Dashboard_AssetsByStatus];");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_Dashboard_AssetsByBrand];");
        }
    }
}
