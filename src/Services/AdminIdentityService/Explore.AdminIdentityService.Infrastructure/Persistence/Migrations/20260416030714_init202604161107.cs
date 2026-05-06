using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Explore.AdminIdentityService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class init202604161107 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AdminUsers_Email",
                table: "AdminUsers");

            migrationBuilder.DropIndex(
                name: "IX_AdminUsers_PhoneNumber",
                table: "AdminUsers");

            migrationBuilder.DropIndex(
                name: "IX_AdminUsers_UserName",
                table: "AdminUsers");

            migrationBuilder.DropIndex(
                name: "IX_AdminUserRoles_AdminUserId_AdminRoleId",
                table: "AdminUserRoles");

            migrationBuilder.DropIndex(
                name: "IX_AdminRoles_Code",
                table: "AdminRoles");

            migrationBuilder.DropIndex(
                name: "IX_AdminRolePermissions_AdminRoleId_AdminPermissionId",
                table: "AdminRolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_AdminPermissions_Code",
                table: "AdminPermissions");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:pgcrypto", ",,");

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_Email",
                table: "AdminUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_PhoneNumber",
                table: "AdminUsers",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_UserName",
                table: "AdminUsers",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminUserRoles_AdminUserId_AdminRoleId",
                table: "AdminUserRoles",
                columns: new[] { "AdminUserId", "AdminRoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminRoles_Code",
                table: "AdminRoles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminRolePermissions_AdminRoleId_AdminPermissionId",
                table: "AdminRolePermissions",
                columns: new[] { "AdminRoleId", "AdminPermissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminPermissions_Code",
                table: "AdminPermissions",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AdminUsers_Email",
                table: "AdminUsers");

            migrationBuilder.DropIndex(
                name: "IX_AdminUsers_PhoneNumber",
                table: "AdminUsers");

            migrationBuilder.DropIndex(
                name: "IX_AdminUsers_UserName",
                table: "AdminUsers");

            migrationBuilder.DropIndex(
                name: "IX_AdminUserRoles_AdminUserId_AdminRoleId",
                table: "AdminUserRoles");

            migrationBuilder.DropIndex(
                name: "IX_AdminRoles_Code",
                table: "AdminRoles");

            migrationBuilder.DropIndex(
                name: "IX_AdminRolePermissions_AdminRoleId_AdminPermissionId",
                table: "AdminRolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_AdminPermissions_Code",
                table: "AdminPermissions");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_Email",
                table: "AdminUsers",
                column: "Email",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_PhoneNumber",
                table: "AdminUsers",
                column: "PhoneNumber",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_UserName",
                table: "AdminUsers",
                column: "UserName",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AdminUserRoles_AdminUserId_AdminRoleId",
                table: "AdminUserRoles",
                columns: new[] { "AdminUserId", "AdminRoleId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AdminRoles_Code",
                table: "AdminRoles",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AdminRolePermissions_AdminRoleId_AdminPermissionId",
                table: "AdminRolePermissions",
                columns: new[] { "AdminRoleId", "AdminPermissionId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AdminPermissions_Code",
                table: "AdminPermissions",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = false");
        }
    }
}


