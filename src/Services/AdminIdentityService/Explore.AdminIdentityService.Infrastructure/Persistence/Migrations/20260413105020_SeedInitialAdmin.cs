using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Explore.AdminIdentityService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
public partial class SeedInitialAdmin : Migration
{
    private const string AdminUserId = "8d6d8c59-8ff6-4f59-8be1-5ea27950b1a6";
    private const string UserName = "bootstrap-admin";
    private const string Email = "bootstrap-admin@example.test";
    private const string DisplayName = "Bootstrap Admin";
    private const string PasswordHash = "AQAAAAIAAYagAAAAEPjHEldRX/I46PNG14V2LqCx9Xzl0w/KwsgGGV88+UgaVgTeAcxtx6eQGhlcz/uReA==";
    private const string CreatedBy = "migration";

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            $"""
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1
                    FROM "AdminUsers"
                    WHERE "UserName" = '{UserName}'
                       OR "Email" = '{Email}')
                THEN
                    INSERT INTO "AdminUsers" (
                        "Id",
                        "UserName",
                        "Email",
                        "DisplayName",
                        "PasswordHash",
                        "IsActive",
                        "LastLoginAt",
                        "CreatedAt",
                        "CreatedBy",
                        "UpdatedAt",
                        "UpdatedBy",
                        "IsDeleted",
                        "Version")
                    VALUES (
                        '{AdminUserId}'::uuid,
                        '{UserName}',
                        '{Email}',
                        '{DisplayName}',
                        '{PasswordHash}',
                        TRUE,
                        NULL,
                        TIMESTAMPTZ '2026-04-13 10:50:20+00',
                        '{CreatedBy}',
                        NULL,
                        NULL,
                        FALSE,
                        1);
                END IF;
            END $$;
            """);
    }

        /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            $"""
            DELETE FROM "AdminUsers"
            WHERE "Id" = '{AdminUserId}'::uuid;
            """);
    }
}
}


