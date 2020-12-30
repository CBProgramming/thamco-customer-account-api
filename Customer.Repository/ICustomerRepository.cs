using Customer.Repository.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Customer.Repository
{
    public interface ICustomerRepository
    {
        public Task<CustomerRepoModel> GetCustomer(int customerId);

        public Task<bool> NewCustomer(CustomerRepoModel newCustomer);

        public Task<bool> EditCustomer(CustomerRepoModel editedCustomer);

        public Task<bool> DeleteCustomer(int customerId);

        public Task<bool> CustomerExists(int customerId);

        public Task<bool> AnonymiseCustomer (CustomerRepoModel anonCustomer);

        public Task<bool> IsCustomerActive(int customerId);

        public Task<bool> MatchingAuthId(int customerId, string authId);
    }
}
