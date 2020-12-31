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
                    new Customer { CustomerId = 1, CustomerAuthId = "b45727f0-bf10-40dc-a687-f5cd025630f2", GivenName = "Chris", FamilyName = "Burrell", AddressOne = "85 Clifton Road", AreaCode = "DL1 5DS", EmailAddress = "chris@example.com", RequestedDeletion = false, Active = true, CanPurchase = true },
                    new Customer { CustomerId = 2, CustomerAuthId = "286fa26e-ae5f-4c5a-b89d-7301fb247d78", GivenName = "Paul", FamilyName = "Mitchell", AddressOne = "85 Clifton Road", AreaCode = "DL1 5DS", EmailAddress = "paul@example.com", RequestedDeletion = false, Active = true, CanPurchase = true },
                    new Customer { CustomerId = 3, CustomerAuthId = "b477a6e4-6607-43c9-8ea0-c2367a5b0360", GivenName = "Jack", FamilyName = "Ferguson", AddressOne = "85 Clifton Road", AreaCode = "DL1 5DS", EmailAddress = "jack@example.com", RequestedDeletion = false, Active = true, CanPurchase = true },
                    new Customer { CustomerId = 4, CustomerAuthId = "9fe723cf-7ac2-4b51-a79a-9e5813fb306a", GivenName = "Carter", FamilyName = "Ridgeway", AddressOne = "85 Clifton Road", AreaCode = "DL1 5DS", EmailAddress = "carter@example.com", RequestedDeletion = false, Active = true, CanPurchase = true },
                    new Customer { CustomerId = 5, CustomerAuthId = "727c783f-ede7-4e53-a365-f8c830e327f4", GivenName = "Karl", FamilyName = "Hall", AddressOne = "85 Clifton Road", AreaCode = "DL1 5DS", EmailAddress = "karl@example.com", RequestedDeletion = false, Active = true, CanPurchase = true }
                );
        }
    }
}
