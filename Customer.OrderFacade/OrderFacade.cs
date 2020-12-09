using Customer.OrderFacade.Models;
using IdentityModel.Client;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Customer.OrderFacade
{
    public class OrderFacade : IOrderFacade
    {
        private readonly IHttpClientFactory _httpClientFactory;
        

        public OrderFacade(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> DeleteCustomer(int customerId)
        {
            var httpClient = _httpClientFactory.CreateClient("CustomerOrderAPI");
            string uri = "/api/Customer/" + customerId;
            if ((await httpClient.DeleteAsync(uri)).IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> EditCustomer(OrderingCustomerDto editedCustomer)
        {
            return await UpdateCustomerOrderService(editedCustomer, false);
        }

        public async Task<bool> NewCustomer(OrderingCustomerDto newCustomer)
        {
            return await UpdateCustomerOrderService(newCustomer, true);
        }

        private async Task<bool> UpdateCustomerOrderService(OrderingCustomerDto customer, bool newCustomer)
        {
            var httpClient = _httpClientFactory.CreateClient("CustomerOrderAPI");
            /*httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
            });*/
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
                uri = uri + customer.CustomerId;
                if ((await httpClient.PutAsJsonAsync<OrderingCustomerDto>(uri, customer)).IsSuccessStatusCode)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
