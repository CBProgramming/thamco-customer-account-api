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
                Customer = anonCustomer;
                return true;
            }
            if (autoFails)
            {
                return false;
            }
            Customer = anonCustomer;
            return true;
        }

        public async Task<bool> CustomerExists(int customerId)
        {
            if (autoSucceeds)
            {
                return true;
            }
            if (autoFails || Customer == null)
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
            return customerId == Customer.CustomerId && Customer.Active;
        }

        public async Task<bool> EditCustomer(CustomerRepoModel editedCustomer)
        {
            if (editedCustomer != null)
            {
                if (Customer != null && editedCustomer.CustomerId == Customer.CustomerId)
                {
                    Customer = editedCustomer;
                    return true;
                }
                else return await NewCustomer(editedCustomer);
            }
            return false;
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

        public async Task<bool> NewCustomer(CustomerRepoModel newCustomer)
        {
            if (newCustomer != null)
            {
                if (Customer == null || newCustomer.CustomerId != Customer.CustomerId)
                {
                    Customer = newCustomer;
                    return true;
                }
                else return await EditCustomer(newCustomer);
            }
            return false;
        }

        public async Task<bool> MatchingAuthId(int customerId, string authId)
        {
            return Customer != null && authId.Equals(Customer.CustomerAuthId);
        }
    }
}
