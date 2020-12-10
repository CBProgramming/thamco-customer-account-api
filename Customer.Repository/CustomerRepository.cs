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
            return await EditCustomer(anonCustomer);
        }

        public async Task<bool> CustomerExists(int customerId)
        {
            return _context.Customers.Any(c => c.CustomerId == customerId);
        }

        public async Task<bool> DeleteCustomer(int customerId)
        {
            return _context.Customers.FirstOrDefault(c => c.CustomerId == customerId).Active;
        }

        public async Task<bool> EditCustomer(CustomerRepoModel editedCustomer)
        {
            if (editedCustomer != null)
            {
                var customer = await _context.Customers.FindAsync(editedCustomer.CustomerId);
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
            return false;
        }

        public async Task<CustomerRepoModel> GetCustomer(int customerId)
        {
            return _mapper.Map<CustomerRepoModel>(_context
                .Customers
                .Where(c => c.Active == true)
                .FirstOrDefault(c => c.CustomerId == customerId));
        }

        public async Task<IList<CustomerRepoModel>> GetCustomersRequestingDeletion()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsCustomerActive(int customerId)
        {
            return _context.Customers.FirstOrDefault(c => c.CustomerId == customerId).Active;
        }

        public async Task<bool> MatchingAuthId(int customerId, string authId)
        {
            return _context.Customers.FirstOrDefault(c => c.CustomerId == customerId).CustomerAuthId.Equals(authId);
        }

        public async Task<bool> NewCustomer(CustomerRepoModel newCustomer)
        {
            if (newCustomer != null)
            {
                try
                {
                    var customer = _mapper.Map<Data.Customer>(newCustomer);
                    _context.Add(customer);
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch (DbUpdateConcurrencyException)
                {

                }
            }
            return false;
        }
    }
}
