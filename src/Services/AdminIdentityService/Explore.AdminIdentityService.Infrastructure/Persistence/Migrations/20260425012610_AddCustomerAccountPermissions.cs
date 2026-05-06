using Explore.AdminIdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Explore.AdminIdentityService.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(AdminIdentityDbContext))]
    [Migration("20260425012610_AddCustomerAccountPermissions")]
    public partial class AddCustomerAccountPermissions : Migration
    {
        private static readonly Guid SuperAdminRoleId = Guid.Parse("6f4f73f2-cc59-48be-8501-97c9bcfd6b8b");
        private static readonly Guid CustomerAccountsPagePermissionId = Guid.Parse("a1c0ec7d-bd0e-4e3a-82d1-7ed2dbbd5080");
        private static readonly Guid CustomerAccountsUpdatePermissionId = Guid.Parse("8bc0c545-d6e0-42c8-a8cc-81970ec4d1a5");
        private static readonly DateTime SeedTime = new(2026, 4, 25, 1, 26, 10, DateTimeKind.Utc);

        private static readonly (Guid Id, string Code, string Name, string Description, int ResourceType, Guid? ParentId)[] Permissions =
        [
            (CustomerAccountsPagePermissionId, "customer_accounts.page", "Customer Accounts Page", "Customer accounts page permission", 1, null),
            (CustomerAccountsUpdatePermissionId, "customer_accounts.update", "Update Customer Account Button", "Update customer account button permission", 2, CustomerAccountsPagePermissionId)
        ];

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var permission in Permissions)
            {
                InsertPermission(
                    migrationBuilder,
                    permission.Id,
                    permission.Code,
                    permission.Name,
                    permission.Description,
                    permission.ResourceType,
                    permission.ParentId);
            }

            foreach (var permission in Permissions)
            {
                InsertRolePermission(migrationBuilder, SuperAdminRoleId, permission.Code);
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var permission in Permissions)
            {
                migrationBuilder.Sql($"""
                    DELETE FROM "AdminRolePermissions" rp
                    USING "AdminPermissions" p
                    WHERE rp."AdminPermissionId" = p."Id"
                      AND rp."AdminRoleId" = '{SuperAdminRoleId}'::uuid
                      AND p."Code" = '{permission.Code}';
                    """);
            }

            migrationBuilder.Sql("""
                DELETE FROM "AdminPermissions"
                WHERE "Code" = 'customer_accounts.update';
                """);

            migrationBuilder.Sql("""
                DELETE FROM "AdminPermissions"
                WHERE "Code" = 'customer_accounts.page';
                """);
        }

        private static void InsertPermission(
            MigrationBuilder migrationBuilder,
            Guid id,
            string code,
            string name,
            string description,
            int resourceType,
            Guid? parentId)
        {
            var parentValue = parentId.HasValue ? $"'{parentId.Value}'::uuid" : "NULL";

            migrationBuilder.Sql($"""
                INSERT INTO "AdminPermissions" ("Id","Code","Name","Description","ResourceType","ParentId","IsActive","CreatedAt","CreatedBy","UpdatedAt","UpdatedBy","IsDeleted","Version")
                SELECT '{id}'::uuid, '{code}', '{name}', '{description}', {resourceType}, {parentValue}, TRUE,  TIMESTAMPTZ '{SeedTime:yyyy-MM-dd HH:mm:ss}+00', 'migration', NULL, NULL, FALSE, 1
                WHERE NOT EXISTS (SELECT 1 FROM "AdminPermissions" WHERE "Code" = '{code}');
                """);
        }

        private static void InsertRolePermission(MigrationBuilder migrationBuilder, Guid roleId, string permissionCode)
        {
            migrationBuilder.Sql($"""
                INSERT INTO "AdminRolePermissions" ("Id","AdminRoleId","AdminPermissionId","CreatedAt","CreatedBy","UpdatedAt","UpdatedBy","IsDeleted","Version")
                SELECT gen_random_uuid(), '{roleId}'::uuid, p."Id", TIMESTAMPTZ '{SeedTime:yyyy-MM-dd HH:mm:ss}+00', 'migration', NULL, NULL, FALSE, 1
                FROM "AdminPermissions" p
                WHERE p."Code" = '{permissionCode}'
                  AND NOT EXISTS (
                      SELECT 1
                      FROM "AdminRolePermissions" rp
                      WHERE rp."AdminRoleId" = '{roleId}'::uuid
                        AND rp."AdminPermissionId" = p."Id");
                """);
        }
    }
}


