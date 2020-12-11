using Microsoft.EntityFrameworkCore.Migrations;

namespace Customer.Data.Migrations
{
    public partial class SeedDataUpdated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "customeraccount",
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 1,
                column: "CanPurchase",
                value: true);

            migrationBuilder.UpdateData(
                schema: "customeraccount",
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 2,
                column: "CanPurchase",
                value: true);

            migrationBuilder.UpdateData(
                schema: "customeraccount",
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 3,
                column: "CanPurchase",
                value: true);

            migrationBuilder.UpdateData(
                schema: "customeraccount",
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 4,
                column: "CanPurchase",
                value: true);

            migrationBuilder.UpdateData(
                schema: "customeraccount",
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 5,
                column: "CanPurchase",
                value: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "customeraccount",
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 1,
                column: "CanPurchase",
                value: false);

            migrationBuilder.UpdateData(
                schema: "customeraccount",
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 2,
                column: "CanPurchase",
                value: false);

            migrationBuilder.UpdateData(
                schema: "customeraccount",
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 3,
                column: "CanPurchase",
                value: false);

            migrationBuilder.UpdateData(
                schema: "customeraccount",
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 4,
                column: "CanPurchase",
                value: false);

            migrationBuilder.UpdateData(
                schema: "customeraccount",
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 5,
                column: "CanPurchase",
                value: false);
        }
    }
}
