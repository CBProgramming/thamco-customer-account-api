using System;
using System.Collections.Generic;
using System.Text;

namespace Customer.Repository.Models
{
    public class CustomerRepoModel
    {
        public int CustomerId { get; set; }

        public string CustomerAuthId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string AddressOne { get; set; }

        public string AddressTwo { get; set; }

        public string Town { get; set; }

        public string County { get; set; }

        public string PostCode { get; set; }

        public string Email { get; set; }

        public string Telephone { get; set; }

        public bool RequestedDeletion { get; set; }

        public bool Active { get; set; }
    }
}
