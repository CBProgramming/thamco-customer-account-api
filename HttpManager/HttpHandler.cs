using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HttpManager
{
    public class HttpHandler : IHttpHandler
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly IAccessTokenGetter _tokenGetter;

        public HttpHandler(IHttpClientFactory httpClientFactory, IConfiguration config, IAccessTokenGetter tokenGetter)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _tokenGetter = tokenGetter;
        }

        public async Task<HttpClient> GetClient(string urlKey, string clientKey, string scopeKey)
        {
            string orderUrl = _config.GetSection(urlKey).Value;
            string authServerUrl = _config.GetSection("CustomerAuthServerUrl").Value;
            string clientSecret = _config.GetSection("ClientSecret").Value;
            string clientId = _config.GetSection("ClientId").Value;
            if (string.IsNullOrEmpty(authServerUrl)
                || string.IsNullOrEmpty(clientSecret)
                || string.IsNullOrEmpty(clientId)
                || string.IsNullOrEmpty(orderUrl))
            {
                return null;
            }
            var client = _httpClientFactory.CreateClient(clientKey);
            client = await _tokenGetter.GetToken(client, authServerUrl, clientId, clientSecret, scopeKey);
            return client;
        }
    }
}
