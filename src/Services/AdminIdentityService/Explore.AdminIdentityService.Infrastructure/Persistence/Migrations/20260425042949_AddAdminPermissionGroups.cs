using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Explore.AdminIdentityService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminPermissionGroups : Migration
    {
        private static readonly Guid SuperAdminRoleId = Guid.Parse("6f4f73f2-cc59-48be-8501-97c9bcfd6b8b");
        private static readonly Guid SystemManagementPermissionId = Guid.Parse("3a7ea4c7-4034-4ab8-b09f-cff4f250ad77");
        private static readonly Guid CustomerCenterPermissionId = Guid.Parse("58db1dce-a57d-4418-bf57-e9deeb3189cc");
        private static readonly Guid MessageCenterPermissionId = Guid.Parse("31fcfc46-e37e-466a-a916-d2c89c1283f0");
        private static readonly DateTime SeedTime = new(2026, 4, 25, 4, 29, 49, DateTimeKind.Utc);

        private static readonly (Guid Id, string Code, string Name, string Description)[] GroupPermissions =
        [
            (SystemManagementPermissionId, "system_management.page", "System Management Page", "System management group permission"),
            (CustomerCenterPermissionId, "customer_center.page", "Customer Center Page", "Customer center group permission"),
            (MessageCenterPermissionId, "message_center.page", "Message Center Page", "Message center group permission")
        ];

        private static readonly (string ChildCode, string ParentCode)[] PermissionGroups =
        [
            ("admin_users.page", "system_management.page"),
            ("admin_roles.page", "system_management.page"),
            ("admin_permissions.page", "system_management.page"),
            ("customer_accounts.page", "customer_center.page"),
            ("message_templates.page", "message_center.page")
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var permission in GroupPermissions)
            {
                InsertPermission(migrationBuilder, permission.Id, permission.Code, permission.Name, permission.Description);
            }

            foreach (var permissionGroup in PermissionGroups)
            {
                ReparentPermission(migrationBuilder, permissionGroup.ChildCode, permissionGroup.ParentCode);
            }

            foreach (var permission in GroupPermissions)
            {
                InsertRolePermission(migrationBuilder, SuperAdminRoleId, permission.Code);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "AdminPermissions"
                SET "ParentId" = NULL
                WHERE "Code" IN (
                    'admin_users.page',
                    'admin_roles.page',
                    'admin_permissions.page',
                    'customer_accounts.page',
                    'message_templates.page'
                );
                """);

            migrationBuilder.Sql(
                """
                DELETE FROM "AdminRolePermissions" rp
                USING "AdminPermissions" p
                WHERE rp."AdminPermissionId" = p."Id"
                  AND p."Code" IN (
                      'system_management.page',
                      'customer_center.page',
                      'message_center.page'
                  );
                """);

            migrationBuilder.Sql(
                """
                DELETE FROM "AdminPermissions"
                WHERE "Code" IN (
                    'system_management.page',
                    'customer_center.page',
                    'message_center.page'
                );
                """);
        }

        private static void InsertPermission(MigrationBuilder migrationBuilder, Guid id, string code, string name, string description)
        {
            migrationBuilder.Sql($"""
                INSERT INTO "AdminPermissions" ("Id","Code","Name","Description","ResourceType","ParentId","IsActive","CreatedAt","CreatedBy","UpdatedAt","UpdatedBy","IsDeleted","Version")
                SELECT '{id}'::uuid, '{code}', '{name}', '{description}', 1, NULL, TRUE,  TIMESTAMPTZ '{SeedTime:yyyy-MM-dd HH:mm:ss}+00', 'migration', NULL, NULL, FALSE, 1
                WHERE NOT EXISTS (SELECT 1 FROM "AdminPermissions" WHERE "Code" = '{code}');
                """);
        }

        private static void ReparentPermission(MigrationBuilder migrationBuilder, string childCode, string parentCode)
        {
            migrationBuilder.Sql($"""
                UPDATE "AdminPermissions" AS child
                SET "ParentId" = parent."Id"
                FROM "AdminPermissions" AS parent
                WHERE child."Code" = '{childCode}'
                  AND parent."Code" = '{parentCode}';
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


