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
        private readonly IHttpHandler _handler;
        private string customerAuthUrl;
        private string customerOrdseringApi;
        private string customerOrdseringScope;
        private string customerUri;

        public OrderFacade(IConfiguration config, IHttpHandler handler)
        {
            _handler = handler;
            if (config != null)
            {
                customerAuthUrl = config.GetSection("CustomerAuthServerUrlKey").Value;
                customerOrdseringApi = config.GetSection("CustomerOrderingAPIKey").Value;
                customerOrdseringScope = config.GetSection("CustomerOrderingScopeKey").Value;
                customerUri = config.GetSection("CustomerUri").Value;
            }
        }

        public async Task<bool> DeleteCustomer(int customerId)
        {
            if (!ValidConfigStrings())
            {
                return false;
            }
            HttpClient httpClient = await _handler.GetClient(customerAuthUrl, 
                customerOrdseringApi, customerOrdseringScope);
            if (httpClient == null)
            {
                return false;
            }
            customerUri = customerUri + customerId;
            if ((await httpClient.DeleteAsync(customerUri)).IsSuccessStatusCode)
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
            if (customer == null 
                || !ValidConfigStrings())
            {
                return false;
            }
            HttpClient httpClient = await _handler.GetClient(customerAuthUrl, 
                customerOrdseringApi, customerOrdseringScope);
            if (httpClient == null)
            {
                return false;
            }
            if (newCustomer)
            {
                if ((await httpClient.PostAsJsonAsync<OrderingCustomerDto>(customerUri, customer))
                    .IsSuccessStatusCode)
                {
                    return true;
                }
            }
            else
            {
                customerUri = customerUri + customer.CustomerId;
                if ((await httpClient.PutAsJsonAsync<OrderingCustomerDto>(customerUri, customer))
                    .IsSuccessStatusCode)
                {
                    return true;
                }
            }
            return false;
        }

        private bool ValidConfigStrings()
        {
            return !string.IsNullOrEmpty(customerAuthUrl)
                    && !string.IsNullOrEmpty(customerOrdseringApi)
                    && !string.IsNullOrEmpty(customerOrdseringScope)
                    && !string.IsNullOrEmpty(customerUri);
        }
    }

    
}
