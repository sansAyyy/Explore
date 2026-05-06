using Explore.AdminIdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Explore.AdminIdentityService.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(AdminIdentityDbContext))]
    [Migration("20260415183000_AddAdminUserPhoneNumber")]
    public partial class AddAdminUserPhoneNumber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "AdminUsers",
                type: "citext",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_PhoneNumber",
                table: "AdminUsers",
                column: "PhoneNumber",
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AdminUsers_PhoneNumber",
                table: "AdminUsers");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "AdminUsers");
        }
    }
}


