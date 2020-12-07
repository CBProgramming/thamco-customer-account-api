using Customer.OrderFacade.Models;
using System;
using System.Threading.Tasks;

namespace Customer.OrderFacade
{
    public interface IOrderFacade
    {
        public Task<bool> NewCustomer(OrderingCustomerDto newCustomer);

        public Task<bool> EditCustomer(OrderingCustomerDto editedCustomer);
    }
}
