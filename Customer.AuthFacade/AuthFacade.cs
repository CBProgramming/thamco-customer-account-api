using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Customer.AuthFacade
{
    public class AuthFacade : IAuthFacade
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public AuthFacade(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }
        public async Task<bool> DeleteAccount(string customerAuthId)
        {
            HttpClient httpClient = await GetClientWithAccessToken();
            string uri = _config.GetSection("AuthUri").Value + "/" + customerAuthId;
            if ((await httpClient.DeleteAsync(uri)).IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        private async Task<HttpClient> GetClientWithAccessToken()
        {
            var client = _httpClientFactory.CreateClient("AuthAPI");
            string authServerUrl = _config.GetSection("CustomerAuthServerUrl").Value;
            string clientSecret = _config.GetSection("ClientSecret").Value;
            string clientId = _config.GetSection("ClientId").Value;
            var disco = await client.GetDiscoveryDocumentAsync(authServerUrl);
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = "customer_auth_customer_api"
            });
            client.SetBearerToken(tokenResponse.AccessToken);
            return client;
        }
    }
}
