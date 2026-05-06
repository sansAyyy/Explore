using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Explore.MessageCenterService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedSingleChannelMessageTemplates : Migration
    {
        private static readonly DateTime SeedTime = new(2026, 4, 24, 3, 38, 25, DateTimeKind.Utc);

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            SeedTemplate(
                migrationBuilder,
                id: "39c0f838-31bc-4c16-ac98-22be4c5d2e61",
                code: "customer_auth.phone_login_code.sms",
                name: "Customer Phone Login Code SMS",
                description: "SMS template for customer phone login verification codes.",
                isEnabled: true,
                channelType: 1,
                titleTemplate: null,
                bodyTemplate: "Your verification code is {{code}}. It expires in {{expireMinutes}} minutes.");

            SeedTemplate(
                migrationBuilder,
                id: "bc94d676-b1ec-4cc8-90dd-0e0b2b7be54e",
                code: "admin_auth.phone_login_code.sms",
                name: "Admin Phone Login Code SMS",
                description: "SMS template for admin phone login verification codes.",
                isEnabled: true,
                channelType: 1,
                titleTemplate: null,
                bodyTemplate: "Your admin verification code is {{code}}. It expires in {{expireMinutes}} minutes.");

            SeedTemplate(
                migrationBuilder,
                id: "43dc31b2-4e96-477e-a709-cb7a45fa44ce",
                code: "admin_identity.admin_user_created.site_message",
                name: "Admin User Created Site Message",
                description: "Site message sent after an admin user is created.",
                isEnabled: true,
                channelType: 3,
                titleTemplate: "Admin account created",
                bodyTemplate: "Admin account {{displayName}} ({{userName}}) was created successfully. Email: {{email}}. Operator: {{operatorAdminUserId}}.");

            SeedTemplate(
                migrationBuilder,
                id: "8874796f-c609-4a74-bef1-c89a18607f27",
                code: "admin_identity.admin_user_activated.site_message",
                name: "Admin User Activated Site Message",
                description: "Site message sent after an admin user is activated.",
                isEnabled: true,
                channelType: 3,
                titleTemplate: "Admin account enabled",
                bodyTemplate: "Admin account {{displayName}} ({{userName}}) was enabled by {{operatorAdminUserId}}. Current status: {{status}}.");

            SeedTemplate(
                migrationBuilder,
                id: "cad68496-0e4b-4570-9c65-bd84650918a5",
                code: "admin_identity.admin_user_deactivated.site_message",
                name: "Admin User Deactivated Site Message",
                description: "Site message sent after an admin user is deactivated.",
                isEnabled: true,
                channelType: 3,
                titleTemplate: "Admin account disabled",
                bodyTemplate: "Admin account {{displayName}} ({{userName}}) was disabled by {{operatorAdminUserId}}. Current status: {{status}}.");

            SeedTemplate(
                migrationBuilder,
                id: "fdf6648d-bd92-4f84-87e2-703722fd95f9",
                code: "admin_identity.admin_user_password_changed_by_admin.site_message",
                name: "Admin Password Reset Site Message",
                description: "Site message sent when an admin password is reset by another admin.",
                isEnabled: true,
                channelType: 3,
                titleTemplate: "Admin password reset",
                bodyTemplate: "The password for admin account {{displayName}} ({{userName}}) was reset by {{operatorAdminUserId}} at {{changedAt}}.");

            SeedTemplate(
                migrationBuilder,
                id: "6a235763-1c4b-4a87-b7a1-ebde2b219eb9",
                code: "order.pay_success.sms",
                name: "Order Pay Success SMS",
                description: "Sample SMS template for order payment success.",
                isEnabled: true,
                channelType: 1,
                titleTemplate: null,
                bodyTemplate: "Your order {{orderNo}} has been paid successfully. Amount: {{amount}}.");

            SeedTemplate(
                migrationBuilder,
                id: "534f4c7a-9380-4296-aa2b-d20f7a4f03be",
                code: "order.pay_success.mini_program",
                name: "Order Pay Success Mini Program",
                description: "Sample mini program template for order payment success.",
                isEnabled: true,
                channelType: 2,
                titleTemplate: "Payment success",
                bodyTemplate: "Order {{orderNo}} was paid successfully. Amount: {{amount}}.");

            SeedTemplate(
                migrationBuilder,
                id: "6fdf32d7-045a-4f0a-bfe1-7bb6a78f6b92",
                code: "order.pay_success.site_message",
                name: "Order Pay Success Site Message",
                description: "Sample site message template for order payment success.",
                isEnabled: true,
                channelType: 3,
                titleTemplate: "Order payment successful",
                bodyTemplate: "Your order {{orderNo}} has been paid successfully. Amount: {{amount}}.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM "MessageTemplates"
                WHERE "Code" IN (
                    'customer_auth.phone_login_code.sms',
                    'admin_auth.phone_login_code.sms',
                    'admin_identity.admin_user_created.site_message',
                    'admin_identity.admin_user_activated.site_message',
                    'admin_identity.admin_user_deactivated.site_message',
                    'admin_identity.admin_user_password_changed_by_admin.site_message',
                    'order.pay_success.sms',
                    'order.pay_success.mini_program',
                    'order.pay_success.site_message'
                );
                """);
        }

        private static void SeedTemplate(
            MigrationBuilder migrationBuilder,
            string id,
            string code,
            string name,
            string description,
            bool isEnabled,
            int channelType,
            string titleTemplate,
            string bodyTemplate)
        {
            var descriptionValue = ToSqlNullable(description);
            var titleValue = ToSqlNullable(titleTemplate);
            var createdBy = "migration";
            var escapedBody = EscapeSql(bodyTemplate);
            var escapedName = EscapeSql(name);
            var escapedCode = EscapeSql(code);
            var escapedCreatedBy = EscapeSql(createdBy);

            migrationBuilder.Sql(
                $"""
                INSERT INTO "MessageTemplates" (
                    "Id",
                    "Code",
                    "Name",
                    "Description",
                    "IsEnabled",
                    "ChannelType",
                    "TitleTemplate",
                    "BodyTemplate",
                    "CreatedAt",
                    "CreatedBy",
                    "UpdatedAt",
                    "UpdatedBy",
                    "IsDeleted",
                    "Version")
                SELECT
                    '{id}'::uuid,
                    '{escapedCode}',
                    '{escapedName}',
                    {descriptionValue},
                    {(isEnabled ? "TRUE" : "FALSE")},
                    {channelType},
                    {titleValue},
                    '{escapedBody}',
                    TIMESTAMPTZ '{SeedTime:yyyy-MM-dd HH:mm:ss}+00',
                    '{escapedCreatedBy}',
                    NULL,
                    NULL,
                    FALSE,
                    1
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM "MessageTemplates"
                    WHERE "Code" = '{escapedCode}'
                );
                """);
        }

        private static string ToSqlNullable(string value)
        {
            return value is null ? "NULL" : $"'{EscapeSql(value)}'";
        }

        private static string EscapeSql(string value)
        {
            return value.Replace("'", "''");
        }
    }
}


