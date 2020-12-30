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
                    new Customer { CustomerId = 1, CustomerAuthId = "eefbace5-3736-4d56-a683-91172561a528", GivenName = "Chris", FamilyName = "Burrell", AddressOne = "85 Clifton Road", AreaCode = "DL1 5DS", EmailAddress = "chris@example.com", RequestedDeletion = false, Active = true, CanPurchase = true },
                    new Customer { CustomerId = 2, CustomerAuthId = "b9196ae2-1892-49ed-9e29-6b8ebf452eaf", GivenName = "Paul", FamilyName = "Mitchell", AddressOne = "85 Clifton Road", AreaCode = "DL1 5DS", EmailAddress = "paul@example.com", RequestedDeletion = false, Active = true, CanPurchase = true },
                    new Customer { CustomerId = 3, CustomerAuthId = "eb0ecafc-9a27-48b7-b73b-4ead95caeea7", GivenName = "Jack", FamilyName = "Ferguson", AddressOne = "85 Clifton Road", AreaCode = "DL1 5DS", EmailAddress = "jack@example.com", RequestedDeletion = false, Active = true, CanPurchase = true },
                    new Customer { CustomerId = 4, CustomerAuthId = "7bc8e757-11d9-4ecb-8ea8-1f436d8490db", GivenName = "Carter", FamilyName = "Ridgeway", AddressOne = "85 Clifton Road", AreaCode = "DL1 5DS", EmailAddress = "carter@example.com", RequestedDeletion = false, Active = true, CanPurchase = true },
                    new Customer { CustomerId = 5, CustomerAuthId = "722e1945-7ade-46ae-9aa8-06f3c8717bd4", GivenName = "Karl", FamilyName = "Hall", AddressOne = "85 Clifton Road", AreaCode = "DL1 5DS", EmailAddress = "karl@example.com", RequestedDeletion = false, Active = true, CanPurchase = true }
                );
        }
    }
}
