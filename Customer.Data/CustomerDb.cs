using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Customer.Data
{
    public class CustomerDb : DbContext
    {
        public DbSet<Customer> Customers { get; set; }

        public CustomerDb(DbContextOptions<CustomerDb> options) : base(options)
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
                .HasData(
                    new Customer { CustomerId = 1, CustomerAuthId = "f756701c-4336-47b1-8317-a16e84bd0059", GivenName = "Chris", FamilyName = "Burrell", AddressOne = "85 Clifton Road", AreaCode = "DL1 5DS", EmailAddress = "chris@example.com", RequestedDeletion = false, Active = true, CanPurchase = true },
                    new Customer { CustomerId = 2, CustomerAuthId = "07dc5dfc-9dad-408c-ba81-ff6a8dd3aec2", GivenName = "Paul", FamilyName = "Mitchell", AddressOne = "85 Clifton Road", AreaCode = "DL1 5DS", EmailAddress = "paul@example.com", RequestedDeletion = false, Active = true, CanPurchase = true },
                    new Customer { CustomerId = 3, CustomerAuthId = "1e3998f7-4ca6-42e0-9c78-8cb030f65f47", GivenName = "Jack", FamilyName = "Ferguson", AddressOne = "85 Clifton Road", AreaCode = "DL1 5DS", EmailAddress = "jack@example.com", RequestedDeletion = false, Active = true, CanPurchase = true },
                    new Customer { CustomerId = 4, CustomerAuthId = "bce3bb9c-5947-4265-8a7d-8588655bbabe", GivenName = "Carter", FamilyName = "Ridgeway", AddressOne = "85 Clifton Road", AreaCode = "DL1 5DS", EmailAddress = "carter@example.com", RequestedDeletion = false, Active = true, CanPurchase = true },
                    new Customer { CustomerId = 5, CustomerAuthId = "fb9e3941-6830-4387-be15-eeac14848c01", GivenName = "Karl", FamilyName = "Hall", AddressOne = "85 Clifton Road", AreaCode = "DL1 5DS", EmailAddress = "karl@example.com", RequestedDeletion = false, Active = true, CanPurchase = true }
                );
        }
    }
}
