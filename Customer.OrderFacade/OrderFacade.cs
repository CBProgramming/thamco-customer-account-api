using Customer.OrderFacade.Models;
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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public OrderFacade(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        private async Task<HttpClient> GetClientWithAccessToken()
        {
            var client = _httpClientFactory.CreateClient("CustomerOrderingAPI");
            string authServerUrl = _config.GetConnectionString("StaffAuthServerUrl");
            string clientSecret = _config.GetConnectionString("ClientSecret");
            string clientId = _config.GetConnectionString("ClientId");
            var disco = await client.GetDiscoveryDocumentAsync(authServerUrl);
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = "customer_ordering_api"
            });
            client.SetBearerToken(tokenResponse.AccessToken);
            return client;
        }

        public async Task<bool> DeleteCustomer(int customerId)
        {
            HttpClient httpClient = await GetClientWithAccessToken();
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
            HttpClient httpClient = await GetClientWithAccessToken();
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
    }
}
