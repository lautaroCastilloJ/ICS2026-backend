using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dsw2025Tpi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddressCheckConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_BillingAddress",
                table: "Orders",
                sql: "LEN(LTRIM(RTRIM([BillingStreet]))) > 0 AND LEN(LTRIM(RTRIM([BillingNumber]))) > 0 AND LEN(LTRIM(RTRIM([BillingCity]))) > 0 AND LEN(LTRIM(RTRIM([BillingProvince]))) > 0 AND ([BillingPostalCode] COLLATE Latin1_General_BIN2 LIKE '[0-9][0-9][0-9][0-9]' OR [BillingPostalCode] COLLATE Latin1_General_BIN2 LIKE '[A-Z][0-9][0-9][0-9][0-9][A-Z][A-Z][A-Z]')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_ShippingAddress",
                table: "Orders",
                sql: "LEN(LTRIM(RTRIM([ShippingStreet]))) > 0 AND LEN(LTRIM(RTRIM([ShippingNumber]))) > 0 AND LEN(LTRIM(RTRIM([ShippingCity]))) > 0 AND LEN(LTRIM(RTRIM([ShippingProvince]))) > 0 AND ([ShippingPostalCode] COLLATE Latin1_General_BIN2 LIKE '[0-9][0-9][0-9][0-9]' OR [ShippingPostalCode] COLLATE Latin1_General_BIN2 LIKE '[A-Z][0-9][0-9][0-9][0-9][A-Z][A-Z][A-Z]')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_BillingAddress",
                table: "Orders");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_ShippingAddress",
                table: "Orders");
        }
    }
}
