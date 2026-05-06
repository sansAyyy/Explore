using Explore.AdminIdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Explore.AdminIdentityService.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(AdminIdentityDbContext))]
    [Migration("20260416090000_AddAdminAuthLogoutPermission")]
    public partial class AddAdminAuthLogoutPermission : Migration
    {
        private static readonly Guid SuperAdminRoleId = Guid.Parse("6f4f73f2-cc59-48be-8501-97c9bcfd6b8b");
        private static readonly Guid LogoutPermissionId = Guid.Parse("2db26906-404f-4da3-84be-a35ccf041bb6");
        private static readonly DateTime SeedTime = new(2026, 4, 16, 9, 0, 0, DateTimeKind.Utc);

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"INSERT INTO \"AdminPermissions\" (\"Id\",\"Code\",\"Name\",\"Description\",\"ResourceType\",\"IsActive\",\"ApiHttpMethod\",\"ApiRouteTemplate\",\"CreatedAt\",\"CreatedBy\",\"UpdatedAt\",\"UpdatedBy\",\"IsDeleted\",\"Version\") SELECT '{LogoutPermissionId}'::uuid, 'api.admin_auth.logout', 'Admin Logout Api', 'Admin logout api permission', 3, TRUE, 'POST', '/api/admin-auth/logout', TIMESTAMPTZ '{SeedTime:yyyy-MM-dd HH:mm:ss}+00', 'migration', NULL, NULL, FALSE, 1 WHERE NOT EXISTS (SELECT 1 FROM \"AdminPermissions\" WHERE \"Code\" = 'api.admin_auth.logout');");
            migrationBuilder.Sql($"INSERT INTO \"AdminRolePermissions\" (\"Id\",\"AdminRoleId\",\"AdminPermissionId\",\"CreatedAt\",\"CreatedBy\",\"UpdatedAt\",\"UpdatedBy\",\"IsDeleted\",\"Version\") SELECT gen_random_uuid(), '{SuperAdminRoleId}'::uuid, p.\"Id\", TIMESTAMPTZ '{SeedTime:yyyy-MM-dd HH:mm:ss}+00', 'migration', NULL, NULL, FALSE, 1 FROM \"AdminPermissions\" p WHERE p.\"Code\" = 'api.admin_auth.logout' AND NOT EXISTS (SELECT 1 FROM \"AdminRolePermissions\" rp WHERE rp.\"AdminRoleId\" = '{SuperAdminRoleId}'::uuid AND rp.\"AdminPermissionId\" = p.\"Id\");");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"DELETE FROM \"AdminRolePermissions\" rp USING \"AdminPermissions\" p WHERE rp.\"AdminPermissionId\" = p.\"Id\" AND rp.\"AdminRoleId\" = '{SuperAdminRoleId}'::uuid AND p.\"Code\" = 'api.admin_auth.logout';");
            migrationBuilder.Sql("DELETE FROM \"AdminPermissions\" WHERE \"Code\" = 'api.admin_auth.logout';");
        }
    }
}


