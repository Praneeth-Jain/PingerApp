using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PingerApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Address",
                table: "PingRecords",
                newName: "IPAddress");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IPAddress",
                table: "PingRecords",
                newName: "Address");
        }
    }
}
