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
        public bool autoSucceeds = false;
        public bool autoFails = false;

        public async Task<bool> AnonymiseCustomer(CustomerRepoModel anonCustomer)
        {
            if (autoSucceeds)
            {
                return true;
            }
            if (autoFails)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> CustomerExists(int customerId)
        {
            if (autoSucceeds)
            {
                return true;
            }
            if (autoFails)
            {
                return false;
            }
            return customerId == Customer.CustomerId;
        }

        public async Task<bool> DeleteCustomer(int customerId)
        {
            if (autoSucceeds)
            {
                return true;
            }
            if (autoFails)
            {
                return false;
            }
            return customerId == Customer.CustomerId;
        }

        public Task<bool> EditCustomer(CustomerRepoModel editedCustomer)
        {
            throw new NotImplementedException();
        }

        public async Task<CustomerRepoModel> GetCustomer(int customerId)
        {
            if (Customer.CustomerId == customerId)
            {
                return Customer;
            }
            return null;
        }

        public Task<IList<CustomerRepoModel>> GetCustomersRequestingDeletion()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsCustomerActive(int customerId)
        {
            return Customer.Active;
        }

        public Task<bool> NewCustomer(CustomerRepoModel newCustomer)
        {
            throw new NotImplementedException();
        }
    }
}
