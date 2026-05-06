using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable enable

namespace Explore.AdminIdentityService.Infrastructure.Persistence.Migrations
{
    public partial class AddRbac : Migration
    {
        private static readonly Guid SuperAdminRoleId = Guid.Parse("6f4f73f2-cc59-48be-8501-97c9bcfd6b8b");
        private static readonly Guid AdminOperatorRoleId = Guid.Parse("f77fef2e-d69d-44cf-8426-931f79cb9807");
        private static readonly Guid SeedAdminUserId = Guid.Parse("8d6d8c59-8ff6-4f59-8be1-5ea27950b1a6");
        private static readonly DateTime SeedTime = new(2026, 4, 15, 10, 0, 0, DateTimeKind.Utc);

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,");

            migrationBuilder.CreateTable(
                name: "AdminPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "citext", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ResourceType = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ApiHttpMethod = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    ApiRouteTemplate = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_AdminPermissions", x => x.Id));

            migrationBuilder.CreateTable(
                name: "AdminRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "citext", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_AdminRoles", x => x.Id));

            migrationBuilder.CreateTable(
                name: "AdminRolePermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminRoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminPermissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminRolePermissions", x => x.Id);
                    table.ForeignKey("FK_AdminRolePermissions_AdminPermissions_AdminPermissionId", x => x.AdminPermissionId, "AdminPermissions", "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_AdminRolePermissions_AdminRoles_AdminRoleId", x => x.AdminRoleId, "AdminRoles", "Id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdminUserRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminRoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminUserRoles", x => x.Id);
                    table.ForeignKey("FK_AdminUserRoles_AdminRoles_AdminRoleId", x => x.AdminRoleId, "AdminRoles", "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_AdminUserRoles_AdminUsers_AdminUserId", x => x.AdminUserId, "AdminUsers", "Id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_AdminPermissions_ApiHttpMethod_ApiRouteTemplate", table: "AdminPermissions", columns: new[] { "ApiHttpMethod", "ApiRouteTemplate" }, unique: true, filter: "\"IsDeleted\" = false AND \"ApiHttpMethod\" IS NOT NULL AND \"ApiRouteTemplate\" IS NOT NULL");
            migrationBuilder.CreateIndex(name: "IX_AdminPermissions_Code", table: "AdminPermissions", column: "Code", unique: true, filter: "\"IsDeleted\" = false");
            migrationBuilder.CreateIndex(name: "IX_AdminRolePermissions_AdminPermissionId", table: "AdminRolePermissions", column: "AdminPermissionId");
            migrationBuilder.CreateIndex(name: "IX_AdminRolePermissions_AdminRoleId_AdminPermissionId", table: "AdminRolePermissions", columns: new[] { "AdminRoleId", "AdminPermissionId" }, unique: true, filter: "\"IsDeleted\" = false");
            migrationBuilder.CreateIndex(name: "IX_AdminRoles_Code", table: "AdminRoles", column: "Code", unique: true, filter: "\"IsDeleted\" = false");
            migrationBuilder.CreateIndex(name: "IX_AdminUserRoles_AdminRoleId", table: "AdminUserRoles", column: "AdminRoleId");
            migrationBuilder.CreateIndex(name: "IX_AdminUserRoles_AdminUserId_AdminRoleId", table: "AdminUserRoles", columns: new[] { "AdminUserId", "AdminRoleId" }, unique: true, filter: "\"IsDeleted\" = false");

            InsertRole(migrationBuilder, SuperAdminRoleId, "super_admin", "Super Admin", "System super administrator role");
            InsertRole(migrationBuilder, AdminOperatorRoleId, "admin_operator", "Admin Operator", "Default operator role");

            foreach (var permission in Permissions)
            {
                InsertPermission(migrationBuilder, permission.Id, permission.Code, permission.Name, permission.Description, permission.ResourceType, permission.ApiHttpMethod, permission.ApiRouteTemplate);
            }

            migrationBuilder.Sql($"INSERT INTO \"AdminUserRoles\" (\"Id\",\"AdminUserId\",\"AdminRoleId\",\"CreatedAt\",\"CreatedBy\",\"UpdatedAt\",\"UpdatedBy\",\"IsDeleted\",\"Version\") SELECT gen_random_uuid(), '{SeedAdminUserId}'::uuid, '{SuperAdminRoleId}'::uuid, TIMESTAMPTZ '{SeedTime:yyyy-MM-dd HH:mm:ss}+00', 'migration', NULL, NULL, FALSE, 1 WHERE NOT EXISTS (SELECT 1 FROM \"AdminUserRoles\" WHERE \"AdminUserId\" = '{SeedAdminUserId}'::uuid AND \"AdminRoleId\" = '{SuperAdminRoleId}'::uuid);");

            foreach (var permission in Permissions)
            {
                InsertRolePermission(migrationBuilder, SuperAdminRoleId, permission.Code);
            }

            foreach (var permissionCode in OperatorPermissionCodes)
            {
                InsertRolePermission(migrationBuilder, AdminOperatorRoleId, permissionCode);
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AdminRolePermissions");
            migrationBuilder.DropTable(name: "AdminUserRoles");
            migrationBuilder.DropTable(name: "AdminPermissions");
            migrationBuilder.DropTable(name: "AdminRoles");
        }

        private static void InsertRole(MigrationBuilder migrationBuilder, Guid id, string code, string name, string description)
        {
            migrationBuilder.Sql($"INSERT INTO \"AdminRoles\" (\"Id\",\"Code\",\"Name\",\"Description\",\"IsActive\",\"CreatedAt\",\"CreatedBy\",\"UpdatedAt\",\"UpdatedBy\",\"IsDeleted\",\"Version\") SELECT '{id}'::uuid, '{code}', '{name}', '{description}', TRUE, TIMESTAMPTZ '{SeedTime:yyyy-MM-dd HH:mm:ss}+00', 'migration', NULL, NULL, FALSE, 1 WHERE NOT EXISTS (SELECT 1 FROM \"AdminRoles\" WHERE \"Code\" = '{code}');");
        }

        private static void InsertPermission(MigrationBuilder migrationBuilder, Guid id, string code, string name, string description, int resourceType, string? apiHttpMethod, string? apiRouteTemplate)
        {
            var methodValue = apiHttpMethod is null ? "NULL" : $"'{apiHttpMethod}'";
            var routeValue = apiRouteTemplate is null ? "NULL" : $"'{apiRouteTemplate}'";
            migrationBuilder.Sql($"INSERT INTO \"AdminPermissions\" (\"Id\",\"Code\",\"Name\",\"Description\",\"ResourceType\",\"IsActive\",\"ApiHttpMethod\",\"ApiRouteTemplate\",\"CreatedAt\",\"CreatedBy\",\"UpdatedAt\",\"UpdatedBy\",\"IsDeleted\",\"Version\") SELECT '{id}'::uuid, '{code}', '{name}', '{description}', {resourceType}, TRUE, {methodValue}, {routeValue}, TIMESTAMPTZ '{SeedTime:yyyy-MM-dd HH:mm:ss}+00', 'migration', NULL, NULL, FALSE, 1 WHERE NOT EXISTS (SELECT 1 FROM \"AdminPermissions\" WHERE \"Code\" = '{code}');");
        }

        private static void InsertRolePermission(MigrationBuilder migrationBuilder, Guid roleId, string permissionCode)
        {
            migrationBuilder.Sql($"INSERT INTO \"AdminRolePermissions\" (\"Id\",\"AdminRoleId\",\"AdminPermissionId\",\"CreatedAt\",\"CreatedBy\",\"UpdatedAt\",\"UpdatedBy\",\"IsDeleted\",\"Version\") SELECT gen_random_uuid(), '{roleId}'::uuid, p.\"Id\", TIMESTAMPTZ '{SeedTime:yyyy-MM-dd HH:mm:ss}+00', 'migration', NULL, NULL, FALSE, 1 FROM \"AdminPermissions\" p WHERE p.\"Code\" = '{permissionCode}' AND NOT EXISTS (SELECT 1 FROM \"AdminRolePermissions\" rp WHERE rp.\"AdminRoleId\" = '{roleId}'::uuid AND rp.\"AdminPermissionId\" = p.\"Id\");");
        }

        private static readonly (Guid Id, string Code, string Name, string Description, int ResourceType, string? ApiHttpMethod, string? ApiRouteTemplate)[] Permissions =
        [
            (Guid.Parse("5d6936bb-b2d8-4f6c-bdb4-86342b2e77ea"), "dashboard.view", "Dashboard View", "Dashboard page permission", 1, null, null),
            (Guid.Parse("8c9cb478-925c-4b5e-b5be-b7f2cd27f4d7"), "admin_users.page", "Admin Users Page", "Admin users page permission", 1, null, null),
            (Guid.Parse("a3db84fe-e57d-4882-90ef-df80cd2bc6eb"), "admin_roles.page", "Admin Roles Page", "Admin roles page permission", 1, null, null),
            (Guid.Parse("9ff7ac2e-2624-43f3-9bbd-9c4d6e26ca89"), "admin_permissions.page", "Admin Permissions Page", "Admin permissions page permission", 1, null, null),
            (Guid.Parse("bd22f82d-24cc-4ad5-898d-4d3c66b4df58"), "admin_authorization.page", "Admin Authorization Page", "Admin authorization page permission", 1, null, null),
            (Guid.Parse("819cf26e-f0d2-4126-9f4d-f4a154ebe0c5"), "admin_users.create", "Create Admin User", "Create admin user button permission", 2, null, null),
            (Guid.Parse("9d801c27-6901-4dfd-b47f-80422697b55d"), "admin_users.update", "Update Admin User", "Update admin user button permission", 2, null, null),
            (Guid.Parse("899fbb4b-b81d-4272-bbbf-f72aee7cfdf9"), "admin_users.delete", "Delete Admin User", "Delete admin user button permission", 2, null, null),
            (Guid.Parse("81c6c8ba-b05f-43cf-b7ae-c1d11232f8ce"), "admin_users.change_password", "Change Admin User Password", "Change admin user password button permission", 2, null, null),
            (Guid.Parse("610bdfe4-847f-4b31-a351-ad60c7990959"), "admin_users.assign_roles", "Assign Admin User Roles", "Assign admin user roles button permission", 2, null, null),
            (Guid.Parse("8c48fd65-d251-4642-b3f0-fbf214fc1fdd"), "admin_roles.create", "Create Admin Role", "Create admin role button permission", 2, null, null),
            (Guid.Parse("c5e23c38-bb41-427c-b942-34f10be109dc"), "admin_roles.update", "Update Admin Role", "Update admin role button permission", 2, null, null),
            (Guid.Parse("50f09ef6-e786-4d0a-9217-eb6ddda2e600"), "admin_roles.delete", "Delete Admin Role", "Delete admin role button permission", 2, null, null),
            (Guid.Parse("60b03ad0-0bbe-41a5-8375-b2cd1ae1f30d"), "admin_roles.assign_permissions", "Assign Role Permissions", "Assign role permissions button permission", 2, null, null),
            (Guid.Parse("86d12cfc-34c2-4727-b6f9-2b8f1d4ca5f9"), "admin_permissions.create", "Create Admin Permission", "Create admin permission button permission", 2, null, null),
            (Guid.Parse("5c6ae072-fb70-4eb7-8335-d03f4f357481"), "admin_permissions.update", "Update Admin Permission", "Update admin permission button permission", 2, null, null),
            (Guid.Parse("7e894c6a-91af-48e2-a18a-2a9d6d4df090"), "admin_permissions.delete", "Delete Admin Permission", "Delete admin permission button permission", 2, null, null),
            (Guid.Parse("99f59f69-7764-4358-91b1-a4580e7682d0"), "current_admin.update_profile", "Update Current Admin Profile", "Update current admin profile button permission", 2, null, null),
            (Guid.Parse("b3a80d2b-23a9-4a52-aa67-49237a5c52fb"), "current_admin.change_password", "Change Current Admin Password", "Change current admin password button permission", 2, null, null),
            (Guid.Parse("d0e9d99a-f95d-4b5a-a5b0-f5a57e7d90ad"), "api.admin_auth.login", "Admin Login Api", "Admin login api permission", 3, "POST", "/api/admin-auth/login"),
            (Guid.Parse("13a87a48-9424-4cf5-a2d6-6be1ef2d835b"), "api.admin_current_user.get", "Get Current Admin Api", "Get current admin api permission", 3, "GET", "/api/admin-current-user"),
            (Guid.Parse("f09071af-a36b-4fd9-af8a-8096238ea15b"), "api.admin_current_user.update", "Update Current Admin Api", "Update current admin api permission", 3, "PUT", "/api/admin-current-user"),
            (Guid.Parse("0efae0a4-b364-468f-a62d-5f4dab7f9580"), "api.admin_current_user.change_password", "Change Current Admin Password Api", "Change current admin password api permission", 3, "PUT", "/api/admin-current-user/password"),
            (Guid.Parse("06d8eb46-f924-48cb-961c-80f2bcab7b08"), "api.admin_authorization.current", "Get Current Authorization Api", "Get current authorization api permission", 3, "GET", "/api/admin-authorization/current"),
            (Guid.Parse("10cc7a6f-6f50-4b76-9ef7-9dc058f7eeb4"), "api.admin_users.list", "List Admin Users Api", "List admin users api permission", 3, "GET", "/api/admin-users"),
            (Guid.Parse("1ce0e13f-c7f9-4768-a1cd-e354ea0fdcd1"), "api.admin_users.get", "Get Admin User Api", "Get admin user api permission", 3, "GET", "/api/admin-users/{id}"),
            (Guid.Parse("5e6677db-f5ab-45db-b620-06ccf69f52f8"), "api.admin_users.create", "Create Admin User Api", "Create admin user api permission", 3, "POST", "/api/admin-users"),
            (Guid.Parse("6c16ea41-bb7d-47eb-807f-31576f33df4f"), "api.admin_users.update", "Update Admin User Api", "Update admin user api permission", 3, "PUT", "/api/admin-users/{id}"),
            (Guid.Parse("b2f8dd06-6d74-493d-84f8-f75065a6f1a0"), "api.admin_users.delete", "Delete Admin User Api", "Delete admin user api permission", 3, "DELETE", "/api/admin-users/{id}"),
            (Guid.Parse("e5a4111a-bf90-4257-875c-1d5df6486134"), "api.admin_users.change_password", "Change Admin User Password Api", "Change admin user password api permission", 3, "PUT", "/api/admin-users/{id}/password"),
            (Guid.Parse("c5e2146d-258d-4d63-93f6-7efbcdf72e23"), "api.admin_users.enable", "Enable Admin User Api", "Enable admin user api permission", 3, "PUT", "/api/admin-users/{id}/enable"),
            (Guid.Parse("86f4a250-c023-46d1-a7bd-c24f2eb0fbb9"), "api.admin_users.disable", "Disable Admin User Api", "Disable admin user api permission", 3, "PUT", "/api/admin-users/{id}/disable"),
            (Guid.Parse("48b8e9e8-e924-471d-b21a-aee7791ae07d"), "api.admin_user_roles.get", "Get Admin User Roles Api", "Get admin user roles api permission", 3, "GET", "/api/admin-users/{userId}/roles"),
            (Guid.Parse("ec6d30db-277e-4f93-a148-0ae41f6a9667"), "api.admin_user_roles.assign", "Assign Admin User Roles Api", "Assign admin user roles api permission", 3, "PUT", "/api/admin-users/{userId}/roles"),
            (Guid.Parse("e96e929e-d022-4882-8f4d-5c25a2fa5fe7"), "api.admin_roles.list", "List Admin Roles Api", "List admin roles api permission", 3, "GET", "/api/admin-roles"),
            (Guid.Parse("6efa9d9d-1bd0-40ba-b7dd-41d1f8f2e2b7"), "api.admin_roles.get", "Get Admin Role Api", "Get admin role api permission", 3, "GET", "/api/admin-roles/{id}"),
            (Guid.Parse("4f2d9488-672d-48cc-b286-1544cb947e17"), "api.admin_roles.create", "Create Admin Role Api", "Create admin role api permission", 3, "POST", "/api/admin-roles"),
            (Guid.Parse("6b6b9c08-77a9-4d56-8e39-959c322792f3"), "api.admin_roles.update", "Update Admin Role Api", "Update admin role api permission", 3, "PUT", "/api/admin-roles/{id}"),
            (Guid.Parse("b5e5ab05-a07c-4f5b-a73f-455ef0c9f0ef"), "api.admin_roles.enable", "Enable Admin Role Api", "Enable admin role api permission", 3, "PUT", "/api/admin-roles/{id}/enable"),
            (Guid.Parse("8fbcce21-3419-44fd-bbc6-ce0f2fca1936"), "api.admin_roles.disable", "Disable Admin Role Api", "Disable admin role api permission", 3, "PUT", "/api/admin-roles/{id}/disable"),
            (Guid.Parse("7707658f-7987-4cf0-8ef1-4691f34e65da"), "api.admin_roles.delete", "Delete Admin Role Api", "Delete admin role api permission", 3, "DELETE", "/api/admin-roles/{id}"),
            (Guid.Parse("7b401b9d-a85f-4699-89ff-c0b1c8f4c5a6"), "api.admin_roles.permissions", "Get Role Permissions Api", "Get role permissions api permission", 3, "GET", "/api/admin-roles/{id}/permissions"),
            (Guid.Parse("d7636ab6-080b-4c73-a3e1-d1cbaf359df9"), "api.admin_roles.assign_permissions", "Assign Role Permissions Api", "Assign role permissions api permission", 3, "PUT", "/api/admin-roles/{id}/permissions"),
            (Guid.Parse("151d1642-a803-4d36-b405-778787f58838"), "api.admin_permissions.list", "List Admin Permissions Api", "List admin permissions api permission", 3, "GET", "/api/admin-permissions"),
            (Guid.Parse("e06557e1-3b2d-48ef-b650-c46574296e6a"), "api.admin_permissions.get", "Get Admin Permission Api", "Get admin permission api permission", 3, "GET", "/api/admin-permissions/{id}"),
            (Guid.Parse("f60d7f10-35c2-43d1-9268-3b9b6ececa60"), "api.admin_permissions.create", "Create Admin Permission Api", "Create admin permission api permission", 3, "POST", "/api/admin-permissions"),
            (Guid.Parse("9fca2d03-0d7d-494b-bf7e-a8af738ae039"), "api.admin_permissions.update", "Update Admin Permission Api", "Update admin permission api permission", 3, "PUT", "/api/admin-permissions/{id}"),
            (Guid.Parse("1ce28b60-6ef0-450d-a2d9-7e57f86f4329"), "api.admin_permissions.enable", "Enable Admin Permission Api", "Enable admin permission api permission", 3, "PUT", "/api/admin-permissions/{id}/enable"),
            (Guid.Parse("0230c6e9-0391-41a0-a812-b6fd37c1718b"), "api.admin_permissions.disable", "Disable Admin Permission Api", "Disable admin permission api permission", 3, "PUT", "/api/admin-permissions/{id}/disable"),
            (Guid.Parse("aadbb2a8-739f-4852-a8f7-d0c167db6200"), "api.admin_permissions.delete", "Delete Admin Permission Api", "Delete admin permission api permission", 3, "DELETE", "/api/admin-permissions/{id}")
        ];

        private static readonly string[] OperatorPermissionCodes =
        [
            "dashboard.view",
            "admin_users.page",
            "admin_authorization.page",
            "current_admin.update_profile",
            "current_admin.change_password",
            "api.admin_current_user.get",
            "api.admin_current_user.update",
            "api.admin_current_user.change_password",
            "api.admin_authorization.current",
            "api.admin_users.list",
            "api.admin_users.get"
        ];
    }
}


