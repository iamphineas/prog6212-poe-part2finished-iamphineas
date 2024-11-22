using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaimPro.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceGeneratedToClaims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "InvoiceGenerated",
                table: "Claims",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceGenerated",
                table: "Claims");
        }
    }
}
