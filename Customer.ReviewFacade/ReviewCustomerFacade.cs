using Customer.ReviewFacade.Models;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Customer.ReviewFacade
{
    public class ReviewCustomerFacade : IReviewCustomerFacade
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public ReviewCustomerFacade(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        private async Task<HttpClient> GetClientWithAccessToken()
        {
            var client = _httpClientFactory.CreateClient("ReviewAPI");
            string authServerUrl = _config.GetSection("CustomerAuthServerUrl").Value;
            string clientSecret = _config.GetSection("ClientSecret").Value;
            string clientId = _config.GetSection("ClientId").Value;
            var disco = await client.GetDiscoveryDocumentAsync(authServerUrl);
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = "customer_review_api"
            });
            client.SetBearerToken(tokenResponse.AccessToken);
            return client;
        }

        public async Task<bool> DeleteCustomer(int customerId)
        {
            HttpClient httpClient = await GetClientWithAccessToken();
            string uri = _config.GetSection("ReviewUri").Value + "/" + customerId;
            if ((await httpClient.DeleteAsync(uri)).IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> EditCustomer(ReviewCustomerDto editedCustomer)
        {
            if (editedCustomer == null)
            {
                return false;
            }
            HttpClient httpClient = await GetClientWithAccessToken();
            string uri = _config.GetSection("ReviewUri").Value + "/" + editedCustomer.CustomerId;
            if ((await httpClient.PutAsJsonAsync<ReviewCustomerDto>(uri, editedCustomer)).IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> NewCustomer(ReviewCustomerDto newCustomer)
        {
            if (newCustomer == null)
            {
                return false;
            }
            HttpClient httpClient = await GetClientWithAccessToken();
            string uri = _config.GetSection("ReviewUri").Value;
            if ((await httpClient.PostAsJsonAsync<ReviewCustomerDto>(uri, newCustomer)).IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }
    }
}
