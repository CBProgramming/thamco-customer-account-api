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
            string clientSecret = _config.GetSection("ClientSecret").Value;
            string clientId = _config.GetSection("ClientId").Value;
            if (string.IsNullOrEmpty(urlKey)
                || string.IsNullOrEmpty(clientSecret)
                || string.IsNullOrEmpty(clientId)
                || string.IsNullOrEmpty(clientKey)
                || string.IsNullOrEmpty(scopeKey))
            {
                return null;
            }
            var client = _httpClientFactory.CreateClient(clientKey);
            if (client == null)
            {
                return null;
            }
            client = await _tokenGetter.GetToken(client, urlKey, clientId, clientSecret, scopeKey);
            if (client == null)
            {
                return null;
            }
            return client;
        }
    }
}
