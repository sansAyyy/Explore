using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Explore.AdminIdentityService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveApiPermissionModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM "AdminRolePermissions"
                WHERE "AdminPermissionId" IN (
                    SELECT "Id"
                    FROM "AdminPermissions"
                    WHERE "ResourceType" = 3
                );
                """);

            migrationBuilder.Sql("""
                DELETE FROM "AdminPermissions"
                WHERE "ResourceType" = 3;
                """);

            migrationBuilder.DropIndex(
                name: "IX_AdminPermissions_ApiHttpMethod_ApiRouteTemplate",
                table: "AdminPermissions");

            migrationBuilder.DropColumn(
                name: "ApiHttpMethod",
                table: "AdminPermissions");

            migrationBuilder.DropColumn(
                name: "ApiRouteTemplate",
                table: "AdminPermissions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiHttpMethod",
                table: "AdminPermissions",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApiRouteTemplate",
                table: "AdminPermissions",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminPermissions_ApiHttpMethod_ApiRouteTemplate",
                table: "AdminPermissions",
                columns: new[] { "ApiHttpMethod", "ApiRouteTemplate" },
                unique: true,
                filter: "\"IsDeleted\" = false AND \"ApiHttpMethod\" IS NOT NULL AND \"ApiRouteTemplate\" IS NOT NULL");
        }
    }
}


