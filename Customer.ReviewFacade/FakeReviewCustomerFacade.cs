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
        public async Task<bool> DeleteCustomer(int customerId)
        {
            return Succeeds;
        }

        public async Task<bool> EditCustomer(ReviewCustomerDto editedCustomer)
        {
            return Succeeds;
        }

        public async Task<bool> NewCustomer(ReviewCustomerDto newCustomer)
        {
            return Succeeds;
        }
    }
}
