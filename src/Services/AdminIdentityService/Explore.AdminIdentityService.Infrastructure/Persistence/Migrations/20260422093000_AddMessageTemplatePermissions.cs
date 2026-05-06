using Explore.AdminIdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Explore.AdminIdentityService.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(AdminIdentityDbContext))]
    [Migration("20260422093000_AddMessageTemplatePermissions")]
    public partial class AddMessageTemplatePermissions : Migration
    {
        private static readonly Guid SuperAdminRoleId = Guid.Parse("6f4f73f2-cc59-48be-8501-97c9bcfd6b8b");
        private static readonly Guid MessageTemplatesPagePermissionId = Guid.Parse("4ce433b1-3b0c-42fe-9f39-7b7269f44cf7");
        private static readonly Guid MessageTemplatesCreatePermissionId = Guid.Parse("aea83f72-5c51-4d00-bfa3-57da269b1ef6");
        private static readonly Guid MessageTemplatesUpdatePermissionId = Guid.Parse("597fe077-1f82-40ae-8603-fcdbe16ce923");
        private static readonly DateTime SeedTime = new(2026, 4, 22, 9, 30, 0, DateTimeKind.Utc);

        private static readonly (Guid Id, string Code, string Name, string Description, int ResourceType, Guid? ParentId)[] Permissions =
        [
            (MessageTemplatesPagePermissionId, "message_templates.page", "Message Templates Page", "Message templates page permission", 1, null),
            (MessageTemplatesCreatePermissionId, "message_templates.create", "Create Message Template Button", "Create message template button permission", 2, MessageTemplatesPagePermissionId),
            (MessageTemplatesUpdatePermissionId, "message_templates.update", "Update Message Template Button", "Update message template button permission", 2, MessageTemplatesPagePermissionId)
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
                WHERE "Code" IN ('message_templates.create', 'message_templates.update');
                """);

            migrationBuilder.Sql("""
                DELETE FROM "AdminPermissions"
                WHERE "Code" = 'message_templates.page';
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


