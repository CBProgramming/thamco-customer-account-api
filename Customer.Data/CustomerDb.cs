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
                    new Customer { CustomerId = 1, GivenName = "Chris", FamilyName = "Burrell", AddressOne = "85 Clifton Road", AreaCode = "DL1 5DS", EmailAddress = "t7145969@live.tees.ac.uk", RequestedDeletion = false, Active = true }
                );
        }
    }
}
