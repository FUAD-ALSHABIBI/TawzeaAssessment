using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TawzeaITAssetAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddITAssetStoredProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE [dbo].[sp_ITAsset_List]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [Id], [Brand], [SerialNumber], [WarrantyExpirationDate], [Status], [CreatedDate]
    FROM [dbo].[ITAssets];
END");

            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE [dbo].[sp_ITAsset_GetById]
    @Id uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [Id], [Brand], [SerialNumber], [WarrantyExpirationDate], [Status], [CreatedDate]
    FROM [dbo].[ITAssets]
    WHERE [Id] = @Id;
END");

            // Id and CreatedDate are server-owned, so they are generated here rather than passed in
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE [dbo].[sp_ITAsset_Create]
    @Brand nvarchar(max),
    @SerialNumber nvarchar(max),
    @WarrantyExpirationDate datetime2,
    @Status nvarchar(20)
AS
BEGIN
    INSERT INTO [dbo].[ITAssets] ([Id], [Brand], [SerialNumber], [WarrantyExpirationDate], [Status], [CreatedDate])
    VALUES (NEWID(), @Brand, @SerialNumber, @WarrantyExpirationDate, @Status, GETDATE());
END");

            // NOCOUNT is left off in Update/Delete so the caller gets the affected row count
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE [dbo].[sp_ITAsset_Update]
    @Id uniqueidentifier,
    @Brand nvarchar(max),
    @SerialNumber nvarchar(max),
    @WarrantyExpirationDate datetime2,
    @Status nvarchar(20)
AS
BEGIN
    UPDATE [dbo].[ITAssets]
    SET [Brand] = @Brand,
        [SerialNumber] = @SerialNumber,
        [WarrantyExpirationDate] = @WarrantyExpirationDate,
        [Status] = @Status
    WHERE [Id] = @Id;
END");

            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE [dbo].[sp_ITAsset_Delete]
    @Id uniqueidentifier
AS
BEGIN
    DELETE FROM [dbo].[ITAssets]
    WHERE [Id] = @Id;
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_ITAsset_Delete];");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_ITAsset_Update];");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_ITAsset_Create];");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_ITAsset_GetById];");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_ITAsset_List];");
        }
    }
}
