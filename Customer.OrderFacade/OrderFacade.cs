using Customer.OrderFacade.Models;
using HttpManager;
using IdentityModel.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Customer.OrderFacade
{
    public class OrderFacade : IOrderFacade
    {
        private readonly IConfiguration _config;
        private readonly IHttpHandler _handler;

        public OrderFacade(IConfiguration config, IHttpHandler handler)
        {
            _config = config;
            _handler = handler;
        }

        public async Task<bool> DeleteCustomer(int customerId)
        {
            HttpClient httpClient = await _handler.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI", "CustomerOrderingScope");
            string uri = "/api/Customer/" + customerId;
            if ((await httpClient.DeleteAsync(uri)).IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> EditCustomer(OrderingCustomerDto editedCustomer)
        {
            if (editedCustomer != null)
            {
                return await UpdateCustomerOrderService(editedCustomer, false);
            }
            return false;
        }

        public async Task<bool> NewCustomer(OrderingCustomerDto newCustomer)
        {
            if (newCustomer != null)
            {
                return await UpdateCustomerOrderService(newCustomer, true);
            }
            return false;
        }

        private async Task<bool> UpdateCustomerOrderService(OrderingCustomerDto customer, bool newCustomer)
        {
            if (customer != null)
            {
                HttpClient httpClient = await _handler.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI", "CustomerOrderingScope");
                string uri = "/api/Customer";
                if (newCustomer)
                {
                    if ((await httpClient.PostAsJsonAsync<OrderingCustomerDto>(uri, customer)).IsSuccessStatusCode)
                    {
                        return true;
                    }
                }
                else
                {
                    uri = uri + "/" + customer.CustomerId;
                    if ((await httpClient.PutAsJsonAsync<OrderingCustomerDto>(uri, customer)).IsSuccessStatusCode)
                    {
                        return true;
                    }
                }
                return false;
            }
            return false;
        }
    }
}
