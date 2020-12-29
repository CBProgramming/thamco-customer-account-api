using Customer.ReviewFacade.Models;
using System;
using System.Threading.Tasks;

namespace Customer.ReviewFacade
{
    public interface IReviewCustomerFacade
    {
        public Task<bool> NewCustomer(ReviewCustomerDto newCustomer);

        public Task<bool> EditCustomer(ReviewCustomerDto editedCustomer);

        public Task<bool> DeleteCustomer(int customerId);
    }
}
