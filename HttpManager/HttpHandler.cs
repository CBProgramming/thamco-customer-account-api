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
            if (string.IsNullOrEmpty(urlKey) || string.IsNullOrEmpty(scopeKey))
            {
                return null;
            }
            string authServerUrl = _config.GetSection(urlKey).Value;
            string clientSecret = _config.GetSection("ClientSecret").Value;
            string clientId = _config.GetSection("ClientId").Value;
            string scope = _config.GetSection(scopeKey).Value;
            if (string.IsNullOrEmpty(clientSecret)
                || string.IsNullOrEmpty(clientId)
                || string.IsNullOrEmpty(clientKey)
                || string.IsNullOrEmpty(scope)
                || string.IsNullOrEmpty(authServerUrl))
            {
                return null;
            }
            var client = _httpClientFactory.CreateClient(clientKey);
            if (client == null)
            {
                return null;
            }
            client = await _tokenGetter.GetToken(client, authServerUrl, clientId, clientSecret, scope);
            if (client == null)
            {
                return null;
            }
            return client;
        }
    }
}
