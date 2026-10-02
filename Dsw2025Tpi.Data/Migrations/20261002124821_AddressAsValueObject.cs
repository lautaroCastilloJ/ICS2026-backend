using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dsw2025Tpi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddressAsValueObject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BillingCity",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BillingNumber",
                table: "Orders",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BillingPostalCode",
                table: "Orders",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BillingProvince",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BillingStreet",
                table: "Orders",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingCity",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingNumber",
                table: "Orders",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingPostalCode",
                table: "Orders",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingProvince",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingStreet",
                table: "Orders",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            // Conserva los datos existentes: la direccion anterior (texto libre)
            // pasa completa a Street, y el resto de los campos queda con un valor
            // de relleno identificable, ya que no se puede inferir del texto.
            migrationBuilder.Sql(@"
UPDATE Orders SET
    ShippingStreet = LEFT(ShippingAddress, 150),
    ShippingNumber = 'S/N',
    ShippingCity = '-',
    ShippingProvince = '-',
    ShippingPostalCode = '0000',
    BillingStreet = LEFT(BillingAddress, 150),
    BillingNumber = 'S/N',
    BillingCity = '-',
    BillingProvince = '-',
    BillingPostalCode = '0000';");

            migrationBuilder.DropColumn(
                name: "BillingAddress",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress",
                table: "Orders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BillingAddress",
                table: "Orders",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress",
                table: "Orders",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
UPDATE Orders SET
    ShippingAddress = LEFT(CONCAT(ShippingStreet, ' ', ShippingNumber, ', ', ShippingCity, ', ', ShippingProvince, ' (CP ', ShippingPostalCode, ')'), 250),
    BillingAddress = LEFT(CONCAT(BillingStreet, ' ', BillingNumber, ', ', BillingCity, ', ', BillingProvince, ' (CP ', BillingPostalCode, ')'), 250);");

            migrationBuilder.DropColumn(
                name: "BillingCity",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BillingNumber",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BillingPostalCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BillingProvince",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BillingStreet",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingCity",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingNumber",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingPostalCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingProvince",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingStreet",
                table: "Orders");
        }
    }
}
