using Customer.OrderFacade.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Customer.OrderFacade
{
    public class FakeFacade : IOrderFacade
    {
        public bool Succeeds = true;

        public Task<bool> DeleteCustomer(int customerId)
        {
            return Succeeds;
        }

        public async Task<bool> EditCustomer(OrderingCustomerDto editedCustomer)
        {
            return Succeeds;
        }

        public async Task<bool> NewCustomer(OrderingCustomerDto newCustomer)
        {
            return Succeeds;
        }
    }
}
