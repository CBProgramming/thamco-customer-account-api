using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Customer.Data
{
    public class CustomerDb : DbContext
    {
        public virtual DbSet<Customer> Customers { get; set; }

        public CustomerDb(DbContextOptions<CustomerDb> options) : base(options)
        {
        }

        public CustomerDb()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("customeraccount");

            modelBuilder.Entity<Customer>()
                .Property(c => c.CustomerId)
                // key is always provided
                .ValueGeneratedNever();

            modelBuilder.Entity<Customer>()
                .HasData(
                    new Customer { CustomerId = 1, CustomerAuthId = "a64c9beb-534a-4b40-a9be-58ed21597cd0", GivenName = "Chris", FamilyName = "Burrell", AddressOne = "85 Clifton Road", AreaCode = "DL1 5DS", EmailAddress = "chris@example.com", RequestedDeletion = false, Active = true, CanPurchase = true },
                    new Customer { CustomerId = 2, CustomerAuthId = "8e689e3c-24b1-400c-a8ad-7435c4fd15b5", GivenName = "Paul", FamilyName = "Mitchell", AddressOne = "85 Clifton Road", AreaCode = "DL1 5DS", EmailAddress = "paul@example.com", RequestedDeletion = false, Active = true, CanPurchase = true },
                    new Customer { CustomerId = 3, CustomerAuthId = "94d6c9b0-b3c8-4ad6-96ed-c7ab43d6dd23", GivenName = "Jack", FamilyName = "Ferguson", AddressOne = "85 Clifton Road", AreaCode = "DL1 5DS", EmailAddress = "jack@example.com", RequestedDeletion = false, Active = true, CanPurchase = true },
                    new Customer { CustomerId = 4, CustomerAuthId = "0313a3ca-e9d0-43c3-a580-ab25c6b224d8", GivenName = "Carter", FamilyName = "Ridgeway", AddressOne = "85 Clifton Road", AreaCode = "DL1 5DS", EmailAddress = "carter@example.com", RequestedDeletion = false, Active = true, CanPurchase = true },
                    new Customer { CustomerId = 5, CustomerAuthId = "8de93d90-7e62-40e9-8032-602f835ee8ee", GivenName = "Karl", FamilyName = "Hall", AddressOne = "85 Clifton Road", AreaCode = "DL1 5DS", EmailAddress = "karl@example.com", RequestedDeletion = false, Active = true, CanPurchase = true }
                );
        }
    }
}
