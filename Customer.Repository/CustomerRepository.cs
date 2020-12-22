using Customer.Repository.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Customer.Data;
using AutoMapper;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Customer.Repository
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly CustomerDb _context;
        private readonly IMapper _mapper;

        public CustomerRepository(CustomerDb context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<bool> AnonymiseCustomer(CustomerRepoModel anonCustomer)
        {
            if (anonCustomer != null)
            {
                return await EditCustomer(anonCustomer);
            }
            return false;
        }

        public async Task<bool> CustomerExists(int customerId)
        {
            return _context.Customers.Any(c => c.CustomerId == customerId);
        }

        public async Task<bool> DeleteCustomer(int customerId)
        {
            //not implemented, anonymise method used instead
            return false;
        }

        public async Task<bool> EditCustomer(CustomerRepoModel editedCustomer)
        {
            if (editedCustomer != null)
            {
                var customer = _context.Customers.FirstOrDefault(c => c.CustomerId == editedCustomer.CustomerId);
                if (customer != null)
                {
                    try
                    {
                        customer.GivenName = editedCustomer.GivenName;
                        customer.FamilyName = editedCustomer.FamilyName;
                        customer.AddressOne = editedCustomer.AddressOne;
                        customer.AddressTwo = editedCustomer.AddressTwo;
                        customer.Town = editedCustomer.Town;
                        customer.State = editedCustomer.State;
                        customer.AreaCode = editedCustomer.AreaCode;
                        customer.Country = editedCustomer.Country;
                        customer.EmailAddress = editedCustomer.EmailAddress;
                        customer.TelephoneNumber = editedCustomer.TelephoneNumber;
                        customer.CanPurchase = editedCustomer.CanPurchase;
                        customer.Active = editedCustomer.Active;
                        await _context.SaveChangesAsync();
                        return true;
                    }
                    catch (DbUpdateConcurrencyException)
                    {

                    }
                }
            }
            return false;
        }

        public async Task<CustomerRepoModel> GetCustomer(int customerId)
        {
            return _mapper.Map<CustomerRepoModel>(_context
                .Customers
                .Where(c => c.Active == true)
                .FirstOrDefault(c => c.CustomerId == customerId));
        }

        public async Task<bool> IsCustomerActive(int customerId)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.CustomerId == customerId);
            if (customer != null)
            {
                return customer.Active;
            }
            return false;
        }

        public async Task<bool> MatchingAuthId(int customerId, string authId)
        {
            if (authId != null)
            {
                var customer = _context.Customers.FirstOrDefault(c => c.CustomerId == customerId);
                if (customer != null)
                {
                    return customer.CustomerAuthId.Equals(authId);
                }
                return false;
            }
            return false;
        }

        public async Task<int> NewCustomer(CustomerRepoModel newCustomer)
        {
            if (newCustomer != null)
            {
                try
                {
                    newCustomer.CustomerId = 0;
                    var customer = _mapper.Map<Data.Customer>(newCustomer);
                    _context.Add(customer);
                    await _context.SaveChangesAsync();
                    return customer.CustomerId;
                }
                catch (DbUpdateConcurrencyException)
                {

                }
            }
            return 0;
        }
    }
}
