using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_CommerceCoreMVC.Migrations
{
    public partial class CheckoutInitial2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateTime",
                table: "Orders",
                newName: "CreatedDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Orders",
                newName: "DateTime");
        }
    }
}
