using Microsoft.EntityFrameworkCore.Migrations;

namespace Customer.Data.Migrations
{
    public partial class initial : Migration
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
                    CustomerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
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
                values: new object[] { 1, true, "85 Clifton Road", null, "DL1 5DS", false, null, null, "t7145969@live.tees.ac.uk", "Burrell", "Chris", false, null, null, null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customers",
                schema: "customeraccount");
        }
    }
}
