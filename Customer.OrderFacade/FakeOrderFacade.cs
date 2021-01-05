using Customer.OrderFacade.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Customer.OrderFacade
{
    public class FakeOrderFacade : IOrderFacade
    {
        public bool Succeeds = true;
        public int CustomerId;
        public OrderingCustomerDto Customer;

        public async Task<bool> DeleteCustomer(int customerId)
        {
            CustomerId = customerId;
            return Succeeds;
        }

        public async Task<bool> EditCustomer(OrderingCustomerDto editedCustomer)
        {
            Customer = editedCustomer;
            return Succeeds;
        }

        public async Task<bool> NewCustomer(OrderingCustomerDto newCustomer)
        {
            Customer = newCustomer;
            return Succeeds;
        }
    }
}
