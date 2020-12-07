using Customer.Repository.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Customer.Repository
{
    public class CustomerRepository : ICustomerRepository
    {
        public Task<bool> DeleteCustomer(int id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EditCustomer(CustomerRepoModel customer)
        {
            throw new NotImplementedException();
        }

        public Task<CustomerRepoModel> GetCustomer(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IList<CustomerRepoModel>> GetCustomers()
        {
            throw new NotImplementedException();
        }

        public Task<IList<CustomerRepoModel>> GetCustomersRequestingDeletion()
        {
            throw new NotImplementedException();
        }

        public Task<bool> NewCustomer(CustomerRepoModel customer)
        {
            throw new NotImplementedException();
        }
    }
}
