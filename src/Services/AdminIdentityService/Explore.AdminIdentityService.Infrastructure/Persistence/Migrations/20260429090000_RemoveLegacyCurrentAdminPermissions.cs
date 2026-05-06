using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Explore.AdminIdentityService.Infrastructure.Persistence.Migrations
{
    public partial class RemoveLegacyCurrentAdminPermissions : Migration
    {
        private static readonly Guid SuperAdminRoleId = Guid.Parse("6f4f73f2-cc59-48be-8501-97c9bcfd6b8b");
        private static readonly Guid AdminOperatorRoleId = Guid.Parse("f77fef2e-d69d-44cf-8426-931f79cb9807");
        private static readonly DateTime SeedTime = new(2026, 4, 29, 9, 0, 0, DateTimeKind.Utc);

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM "AdminRolePermissions" rp
                USING "AdminPermissions" p
                WHERE rp."AdminPermissionId" = p."Id"
                  AND p."Code" IN (
                      'admin_authorization.page',
                      'current_admin.update_profile',
                      'current_admin.change_password'
                  );
                """);

            migrationBuilder.Sql(
                """
                DELETE FROM "AdminPermissions"
                WHERE "Code" IN (
                    'admin_authorization.page',
                    'current_admin.update_profile',
                    'current_admin.change_password'
                );
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            InsertPermission(
                migrationBuilder,
                Guid.Parse("bd22f82d-24cc-4ad5-898d-4d3c66b4df58"),
                "admin_authorization.page",
                "Admin Authorization Page",
                "Admin authorization page permission",
                1);

            InsertPermission(
                migrationBuilder,
                Guid.Parse("99f59f69-7764-4358-91b1-a4580e7682d0"),
                "current_admin.update_profile",
                "Update Current Admin Profile",
                "Update current admin profile button permission",
                2);

            InsertPermission(
                migrationBuilder,
                Guid.Parse("b3a80d2b-23a9-4a52-aa67-49237a5c52fb"),
                "current_admin.change_password",
                "Change Current Admin Password",
                "Change current admin password button permission",
                2);

            InsertRolePermission(migrationBuilder, SuperAdminRoleId, "admin_authorization.page");
            InsertRolePermission(migrationBuilder, SuperAdminRoleId, "current_admin.update_profile");
            InsertRolePermission(migrationBuilder, SuperAdminRoleId, "current_admin.change_password");

            InsertRolePermission(migrationBuilder, AdminOperatorRoleId, "admin_authorization.page");
            InsertRolePermission(migrationBuilder, AdminOperatorRoleId, "current_admin.update_profile");
            InsertRolePermission(migrationBuilder, AdminOperatorRoleId, "current_admin.change_password");
        }

        private static void InsertPermission(
            MigrationBuilder migrationBuilder,
            Guid id,
            string code,
            string name,
            string description,
            int resourceType)
        {
            migrationBuilder.Sql($"""
                INSERT INTO "AdminPermissions" ("Id","Code","Name","Description","ResourceType","ParentId","IsActive","CreatedAt","CreatedBy","UpdatedAt","UpdatedBy","IsDeleted","Version")
                SELECT '{id}'::uuid, '{code}', '{name}', '{description}', {resourceType}, NULL, TRUE, TIMESTAMPTZ '{SeedTime:yyyy-MM-dd HH:mm:ss}+00', 'migration', NULL, NULL, FALSE, 1
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


