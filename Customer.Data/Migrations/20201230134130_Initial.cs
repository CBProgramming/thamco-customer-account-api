using Microsoft.EntityFrameworkCore.Migrations;

namespace Customer.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "customeraccount");

            migrationBuilder.CreateTable(
                name: "Customers",
                schema: "customeraccount",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CustomerAuthId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GivenName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FamilyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressOne = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressTwo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Town = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AreaCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TelephoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestedDeletion = table.Column<bool>(type: "bit", nullable: false),
                    CanPurchase = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerId);
                });

            migrationBuilder.InsertData(
                schema: "customeraccount",
                table: "Customers",
                columns: new[] { "CustomerId", "Active", "AddressOne", "AddressTwo", "AreaCode", "CanPurchase", "Country", "CustomerAuthId", "EmailAddress", "FamilyName", "GivenName", "RequestedDeletion", "State", "TelephoneNumber", "Town" },
                values: new object[,]
                {
                    { 1, true, "85 Clifton Road", null, "DL1 5DS", true, null, "eefbace5-3736-4d56-a683-91172561a528", "chris@example.com", "Burrell", "Chris", false, null, null, null },
                    { 2, true, "85 Clifton Road", null, "DL1 5DS", true, null, "b9196ae2-1892-49ed-9e29-6b8ebf452eaf", "paul@example.com", "Mitchell", "Paul", false, null, null, null },
                    { 3, true, "85 Clifton Road", null, "DL1 5DS", true, null, "eb0ecafc-9a27-48b7-b73b-4ead95caeea7", "jack@example.com", "Ferguson", "Jack", false, null, null, null },
                    { 4, true, "85 Clifton Road", null, "DL1 5DS", true, null, "7bc8e757-11d9-4ecb-8ea8-1f436d8490db", "carter@example.com", "Ridgeway", "Carter", false, null, null, null },
                    { 5, true, "85 Clifton Road", null, "DL1 5DS", true, null, "722e1945-7ade-46ae-9aa8-06f3c8717bd4", "karl@example.com", "Hall", "Karl", false, null, null, null }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customers",
                schema: "customeraccount");
        }
    }
}
