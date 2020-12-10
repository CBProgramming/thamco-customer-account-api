using Microsoft.EntityFrameworkCore.Migrations;

namespace Customer.Data.Migrations
{
    public partial class authIdsAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "customeraccount",
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 1,
                columns: new[] { "CustomerAuthId", "EmailAddress" },
                values: new object[] { "f756701c-4336-47b1-8317-a16e84bd0059", "chris@example.com" });

            migrationBuilder.InsertData(
                schema: "customeraccount",
                table: "Customers",
                columns: new[] { "CustomerId", "Active", "AddressOne", "AddressTwo", "AreaCode", "CanPurchase", "Country", "CustomerAuthId", "EmailAddress", "FamilyName", "GivenName", "RequestedDeletion", "State", "TelephoneNumber", "Town" },
                values: new object[,]
                {
                    { 2, true, "85 Clifton Road", null, "DL1 5DS", false, null, "07dc5dfc-9dad-408c-ba81-ff6a8dd3aec2", "paul@example.com", "Mitchell", "Paul", false, null, null, null },
                    { 3, true, "85 Clifton Road", null, "DL1 5DS", false, null, "1e3998f7-4ca6-42e0-9c78-8cb030f65f47", "jack@example.com", "Ferguson", "Jack", false, null, null, null },
                    { 4, true, "85 Clifton Road", null, "DL1 5DS", false, null, "bce3bb9c-5947-4265-8a7d-8588655bbabe", "carter@example.com", "Ridgeway", "Carter", false, null, null, null },
                    { 5, true, "85 Clifton Road", null, "DL1 5DS", false, null, "fb9e3941-6830-4387-be15-eeac14848c01", "karl@example.com", "Hall", "Karl", false, null, null, null }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "customeraccount",
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "customeraccount",
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "customeraccount",
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "customeraccount",
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 5);

            migrationBuilder.UpdateData(
                schema: "customeraccount",
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 1,
                columns: new[] { "CustomerAuthId", "EmailAddress" },
                values: new object[] { null, "t7145969@live.tees.ac.uk" });
        }
    }
}
