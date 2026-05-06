using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Explore.AdminIdentityService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminPermissionHierarchy : Migration
    {
        private static readonly Guid SuperAdminRoleId = Guid.Parse("6f4f73f2-cc59-48be-8501-97c9bcfd6b8b");
        private static readonly DateTime SeedTime = new(2026, 4, 22, 2, 3, 42, DateTimeKind.Utc);
        private static readonly (Guid Id, string Code, string Name, string Description, int ResourceType, string ApiHttpMethod, string ApiRouteTemplate)[] ApiPermissions =
        [
            (Guid.Parse("c5b72fdb-911d-4ff7-b90d-50be947685a7"), "api.admin_permissions.roots", "Get Admin Permission Roots Api", "Get admin permission roots api permission", 3, "GET", "/api/admin-permissions/roots"),
            (Guid.Parse("95dc3848-5e39-4ad7-beb3-ab3c9199542d"), "api.admin_permissions.descendants", "Get Admin Permission Descendants Api", "Get admin permission descendants api permission", 3, "GET", "/api/admin-permissions/{id}/descendants")
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "AdminPermissions",
                type: "uuid",
                nullable: true);

            foreach (var permission in ApiPermissions)
            {
                InsertPermission(
                    migrationBuilder,
                    permission.Id,
                    permission.Code,
                    permission.Name,
                    permission.Description,
                    permission.ResourceType,
                    permission.ApiHttpMethod,
                    permission.ApiRouteTemplate);
            }

            migrationBuilder.Sql(
                """
                UPDATE "AdminPermissions" AS child
                SET "ParentId" = root."Id"
                FROM "AdminPermissions" AS root
                WHERE child."ParentId" IS NULL
                  AND child."Code" <> root."Code"
                  AND root."Code" = CASE
                      WHEN child."Code" LIKE 'api.%' THEN split_part(child."Code", '.', 2) || '.page'
                      ELSE split_part(child."Code", '.', 1) || '.page'
                  END;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_AdminPermissions_ParentId",
                table: "AdminPermissions",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdminPermissions_AdminPermissions_ParentId",
                table: "AdminPermissions",
                column: "ParentId",
                principalTable: "AdminPermissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            foreach (var permission in ApiPermissions)
            {
                InsertRolePermission(migrationBuilder, SuperAdminRoleId, permission.Code);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var permission in ApiPermissions)
            {
                migrationBuilder.Sql($"""
                    DELETE FROM "AdminRolePermissions" rp
                    USING "AdminPermissions" p
                    WHERE rp."AdminPermissionId" = p."Id"
                      AND rp."AdminRoleId" = '{SuperAdminRoleId}'::uuid
                      AND p."Code" = '{permission.Code}';
                    """);

                migrationBuilder.Sql($"""
                    DELETE FROM "AdminPermissions"
                    WHERE "Code" = '{permission.Code}';
                    """);
            }

            migrationBuilder.DropForeignKey(
                name: "FK_AdminPermissions_AdminPermissions_ParentId",
                table: "AdminPermissions");

            migrationBuilder.DropIndex(
                name: "IX_AdminPermissions_ParentId",
                table: "AdminPermissions");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "AdminPermissions");
        }

        private static void InsertPermission(MigrationBuilder migrationBuilder, Guid id, string code, string name, string description, int resourceType, string apiHttpMethod, string apiRouteTemplate)
        {
            migrationBuilder.Sql($"""
                INSERT INTO "AdminPermissions" ("Id","Code","Name","Description","ResourceType","IsActive","ApiHttpMethod","ApiRouteTemplate","CreatedAt","CreatedBy","UpdatedAt","UpdatedBy","IsDeleted","Version")
                SELECT '{id}'::uuid, '{code}', '{name}', '{description}', {resourceType}, TRUE, '{apiHttpMethod}', '{apiRouteTemplate}', TIMESTAMPTZ '{SeedTime:yyyy-MM-dd HH:mm:ss}+00', 'migration', NULL, NULL, FALSE, 1
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


