using Customer.Repository.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Customer.Repository
{
    public interface ICustomerRepository
    {
        public Task<CustomerRepoModel> GetCustomer(int id);

        public Task<IList<CustomerRepoModel>> GetCustomers();

        public Task<IList<CustomerRepoModel>> GetCustomersRequestingDeletion();

        public Task<bool> NewCustomer(CustomerRepoModel customer);

        public Task<bool> EditCustomer(CustomerRepoModel customer);

        public Task<bool> DeleteCustomer(int id);
    }
}
