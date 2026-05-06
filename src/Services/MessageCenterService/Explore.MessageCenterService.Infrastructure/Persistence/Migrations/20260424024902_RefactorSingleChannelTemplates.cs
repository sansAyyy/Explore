using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Explore.MessageCenterService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefactorSingleChannelTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "MessageTemplates",
                newName: "MessageTemplates_Legacy");

            migrationBuilder.Sql(
                """
                ALTER TABLE "MessageTemplates_Legacy"
                RENAME CONSTRAINT "PK_MessageTemplates" TO "PK_MessageTemplates_Legacy";
                """);

            migrationBuilder.RenameIndex(
                name: "IX_MessageTemplates_Code",
                table: "MessageTemplates_Legacy",
                newName: "IX_MessageTemplates_Legacy_Code");

            migrationBuilder.CreateTable(
                name: "MessageTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ChannelType = table.Column<int>(type: "integer", nullable: false),
                    TitleTemplate = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    BodyTemplate = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageTemplates_Code",
                table: "MessageTemplates",
                column: "Code",
                unique: true);

            migrationBuilder.Sql(
                """
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
                    channel."Id",
                    legacy."Code" ||
                        CASE channel."ChannelType"
                            WHEN 1 THEN '.sms'
                            WHEN 2 THEN '.mini_program'
                            WHEN 3 THEN '.site_message'
                            ELSE '.channel_' || channel."ChannelType"::text
                        END,
                    legacy."Name",
                    legacy."Description",
                    legacy."IsEnabled" AND channel."IsEnabled",
                    channel."ChannelType",
                    channel."TitleTemplate",
                    channel."BodyTemplate",
                    legacy."CreatedAt",
                    legacy."CreatedBy",
                    legacy."UpdatedAt",
                    legacy."UpdatedBy",
                    legacy."IsDeleted",
                    legacy."Version"
                FROM "MessageTemplates_Legacy" AS legacy
                INNER JOIN "MessageTemplateChannels" AS channel ON legacy."Id" = channel."TemplateId";
                """);

            migrationBuilder.DropTable(
                name: "MessageTemplateChannels");

            migrationBuilder.DropTable(
                name: "MessageTemplates_Legacy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "MessageTemplates",
                newName: "MessageTemplates_SingleChannel");

            migrationBuilder.Sql(
                """
                ALTER TABLE "MessageTemplates_SingleChannel"
                RENAME CONSTRAINT "PK_MessageTemplates" TO "PK_MessageTemplates_SingleChannel";
                """);

            migrationBuilder.RenameIndex(
                name: "IX_MessageTemplates_Code",
                table: "MessageTemplates_SingleChannel",
                newName: "IX_MessageTemplates_SingleChannel_Code");

            migrationBuilder.CreateTable(
                name: "MessageTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageTemplates_Code",
                table: "MessageTemplates",
                column: "Code",
                unique: true);

            migrationBuilder.CreateTable(
                name: "MessageTemplateChannels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BodyTemplate = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ChannelType = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    TitleTemplate = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageTemplateChannels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageTemplateChannels_MessageTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "MessageTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageTemplateChannels_TemplateId_ChannelType",
                table: "MessageTemplateChannels",
                columns: new[] { "TemplateId", "ChannelType" },
                unique: true);

            migrationBuilder.Sql(
                """
                WITH grouped_templates AS (
                    SELECT DISTINCT ON ("BaseCode")
                        "BaseCode",
                        "Id",
                        "Name",
                        "Description",
                        "IsEnabled",
                        "CreatedAt",
                        "CreatedBy",
                        "UpdatedAt",
                        "UpdatedBy",
                        "IsDeleted",
                        "Version"
                    FROM (
                        SELECT
                            CASE
                                WHEN RIGHT(template."Code", 4) = '.sms' THEN LEFT(template."Code", LENGTH(template."Code") - 4)
                                WHEN RIGHT(template."Code", 13) = '.mini_program' THEN LEFT(template."Code", LENGTH(template."Code") - 13)
                                WHEN RIGHT(template."Code", 13) = '.site_message' THEN LEFT(template."Code", LENGTH(template."Code") - 13)
                                ELSE template."Code"
                            END AS "BaseCode",
                            template."Id",
                            template."Name",
                            template."Description",
                            template."IsEnabled",
                            template."CreatedAt",
                            template."CreatedBy",
                            template."UpdatedAt",
                            template."UpdatedBy",
                            template."IsDeleted",
                            template."Version"
                        FROM "MessageTemplates_SingleChannel" AS template
                    ) AS normalized
                    ORDER BY "BaseCode", "CreatedAt", "Id"
                )
                INSERT INTO "MessageTemplates" (
                    "Id",
                    "Code",
                    "Name",
                    "Description",
                    "IsEnabled",
                    "CreatedAt",
                    "CreatedBy",
                    "UpdatedAt",
                    "UpdatedBy",
                    "IsDeleted",
                    "Version")
                SELECT
                    grouped."Id",
                    grouped."BaseCode",
                    grouped."Name",
                    grouped."Description",
                    grouped."IsEnabled",
                    grouped."CreatedAt",
                    grouped."CreatedBy",
                    grouped."UpdatedAt",
                    grouped."UpdatedBy",
                    grouped."IsDeleted",
                    grouped."Version"
                FROM grouped_templates AS grouped;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO "MessageTemplateChannels" (
                    "Id",
                    "BodyTemplate",
                    "ChannelType",
                    "IsEnabled",
                    "TemplateId",
                    "TitleTemplate")
                SELECT
                    template."Id",
                    template."BodyTemplate",
                    template."ChannelType",
                    template."IsEnabled",
                    grouped."Id",
                    template."TitleTemplate"
                FROM "MessageTemplates_SingleChannel" AS template
                INNER JOIN (
                    SELECT DISTINCT ON ("BaseCode")
                        "BaseCode",
                        "Id"
                    FROM (
                        SELECT
                            CASE
                                WHEN RIGHT(inner_template."Code", 4) = '.sms' THEN LEFT(inner_template."Code", LENGTH(inner_template."Code") - 4)
                                WHEN RIGHT(inner_template."Code", 13) = '.mini_program' THEN LEFT(inner_template."Code", LENGTH(inner_template."Code") - 13)
                                WHEN RIGHT(inner_template."Code", 13) = '.site_message' THEN LEFT(inner_template."Code", LENGTH(inner_template."Code") - 13)
                                ELSE inner_template."Code"
                            END AS "BaseCode",
                            inner_template."Id",
                            inner_template."CreatedAt"
                        FROM "MessageTemplates_SingleChannel" AS inner_template
                    ) AS normalized
                    ORDER BY "BaseCode", "CreatedAt", "Id"
                ) AS grouped
                    ON grouped."BaseCode" = CASE
                        WHEN RIGHT(template."Code", 4) = '.sms' THEN LEFT(template."Code", LENGTH(template."Code") - 4)
                        WHEN RIGHT(template."Code", 13) = '.mini_program' THEN LEFT(template."Code", LENGTH(template."Code") - 13)
                        WHEN RIGHT(template."Code", 13) = '.site_message' THEN LEFT(template."Code", LENGTH(template."Code") - 13)
                        ELSE template."Code"
                    END;
                """);

            migrationBuilder.DropTable(
                name: "MessageTemplates_SingleChannel");
        }
    }
}


