using Customer.Repository.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Customer.Repository
{
    public class FakeCustomerRepository : ICustomerRepository
    {
        public CustomerRepoModel Customer { get; set; }

        public Task<bool> AnonymiseCustomer(CustomerRepoModel anonCustomer)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CustomerExists(int customerId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteCustomer(int customerId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EditCustomer(CustomerRepoModel editedCustomer)
        {
            throw new NotImplementedException();
        }

        public Task<CustomerRepoModel> GetCustomer(int customerId)
        {
            throw new NotImplementedException();
        }

        public Task<IList<CustomerRepoModel>> GetCustomersRequestingDeletion()
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsCustomerActive(int customerId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> NewCustomer(CustomerRepoModel newCustomer)
        {
            throw new NotImplementedException();
        }
    }
}
