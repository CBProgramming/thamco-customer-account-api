using Customer.ReviewFacade.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Customer.ReviewFacade
{
    public class FakeReviewCustomerFacade : IReviewCustomerFacade
    {
        public bool Succeeds = true;
        public int CustomerId;
        public ReviewCustomerDto Customer;

        public async Task<bool> DeleteCustomer(int customerId)
        {
            CustomerId = customerId;
            return Succeeds;
        }

        public async Task<bool> EditCustomer(ReviewCustomerDto editedCustomer)
        {
            Customer = editedCustomer;
            return Succeeds;
        }

        public async Task<bool> NewCustomer(ReviewCustomerDto newCustomer)
        {
            Customer = newCustomer;
            return Succeeds;
        }
    }
}
