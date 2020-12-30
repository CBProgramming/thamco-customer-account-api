﻿// <auto-generated />
using Customer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Customer.Data.Migrations
{
    [DbContext(typeof(CustomerDb))]
    partial class CustomerDbModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("customeraccount")
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("Customer.Data.Customer", b =>
                {
                    b.Property<int>("CustomerId")
                        .HasColumnType("int");

                    b.Property<bool>("Active")
                        .HasColumnType("bit");

                    b.Property<string>("AddressOne")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("AddressTwo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("AreaCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("CanPurchase")
                        .HasColumnType("bit");

                    b.Property<string>("Country")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CustomerAuthId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EmailAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FamilyName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("GivenName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("RequestedDeletion")
                        .HasColumnType("bit");

                    b.Property<string>("State")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TelephoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Town")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("CustomerId");

                    b.ToTable("Customers");

                    b.HasData(
                        new
                        {
                            CustomerId = 1,
                            Active = true,
                            AddressOne = "85 Clifton Road",
                            AreaCode = "DL1 5DS",
                            CanPurchase = true,
                            CustomerAuthId = "eefbace5-3736-4d56-a683-91172561a528",
                            EmailAddress = "chris@example.com",
                            FamilyName = "Burrell",
                            GivenName = "Chris",
                            RequestedDeletion = false
                        },
                        new
                        {
                            CustomerId = 2,
                            Active = true,
                            AddressOne = "85 Clifton Road",
                            AreaCode = "DL1 5DS",
                            CanPurchase = true,
                            CustomerAuthId = "b9196ae2-1892-49ed-9e29-6b8ebf452eaf",
                            EmailAddress = "paul@example.com",
                            FamilyName = "Mitchell",
                            GivenName = "Paul",
                            RequestedDeletion = false
                        },
                        new
                        {
                            CustomerId = 3,
                            Active = true,
                            AddressOne = "85 Clifton Road",
                            AreaCode = "DL1 5DS",
                            CanPurchase = true,
                            CustomerAuthId = "eb0ecafc-9a27-48b7-b73b-4ead95caeea7",
                            EmailAddress = "jack@example.com",
                            FamilyName = "Ferguson",
                            GivenName = "Jack",
                            RequestedDeletion = false
                        },
                        new
                        {
                            CustomerId = 4,
                            Active = true,
                            AddressOne = "85 Clifton Road",
                            AreaCode = "DL1 5DS",
                            CanPurchase = true,
                            CustomerAuthId = "7bc8e757-11d9-4ecb-8ea8-1f436d8490db",
                            EmailAddress = "carter@example.com",
                            FamilyName = "Ridgeway",
                            GivenName = "Carter",
                            RequestedDeletion = false
                        },
                        new
                        {
                            CustomerId = 5,
                            Active = true,
                            AddressOne = "85 Clifton Road",
                            AreaCode = "DL1 5DS",
                            CanPurchase = true,
                            CustomerAuthId = "722e1945-7ade-46ae-9aa8-06f3c8717bd4",
                            EmailAddress = "karl@example.com",
                            FamilyName = "Hall",
                            GivenName = "Karl",
                            RequestedDeletion = false
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
