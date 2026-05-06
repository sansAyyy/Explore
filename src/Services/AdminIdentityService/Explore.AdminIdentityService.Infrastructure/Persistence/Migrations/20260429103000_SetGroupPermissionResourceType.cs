using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Explore.AdminIdentityService.Infrastructure.Persistence.Migrations
{
    public partial class SetGroupPermissionResourceType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "AdminPermissions"
                SET "ResourceType" = 3
                WHERE "Code" IN (
                    'system_management.page',
                    'customer_center.page',
                    'message_center.page'
                );
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "AdminPermissions"
                SET "ResourceType" = 1
                WHERE "Code" IN (
                    'system_management.page',
                    'customer_center.page',
                    'message_center.page'
                );
                """);
        }
    }
}


