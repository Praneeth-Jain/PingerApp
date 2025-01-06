using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PingerApp.Migrations
{
    /// <inheritdoc />
    public partial class IPtableAddedv4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_IPadresses",
                table: "IPadresses");

            migrationBuilder.AddColumn<int>(
                name: "ID",
                table: "IPadresses",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_IPadresses",
                table: "IPadresses",
                column: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_IPadresses",
                table: "IPadresses");

            migrationBuilder.DropColumn(
                name: "ID",
                table: "IPadresses");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IPadresses",
                table: "IPadresses",
                column: "IPAddress");
        }
    }
}
