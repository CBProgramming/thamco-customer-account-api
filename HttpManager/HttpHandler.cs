using IdentityModel.Client;
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
        private readonly ClientCredentialsTokenRequest _tokenRequest;
        private readonly IUnmockablesWrapper _unmockablesWrapper;

        public HttpHandler(IHttpClientFactory httpClientFactory, IConfiguration config,
            ClientCredentialsTokenRequest tokenRequest, IUnmockablesWrapper unmockablesWrapper)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _tokenRequest = tokenRequest;
            _unmockablesWrapper = unmockablesWrapper;
        }

        public async Task<HttpClient> GetClient(string urlKey, string clientKey, string scopeKey)
        {
            if (string.IsNullOrEmpty(urlKey) || string.IsNullOrEmpty(scopeKey))
            {
                return null;
            }
            string authServerUrl = _config.GetSection(urlKey).Value;
            string scope = _config.GetSection(scopeKey).Value;
            if (string.IsNullOrEmpty(clientKey)
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
            /*client = await _tokenGetter.GetToken(client, authServerUrl, scope);
            if (client == null)
            {
                return null;
            }*/
            var disco = await _unmockablesWrapper.GetDiscoveryDocumentAsync(client,authServerUrl);
            _tokenRequest.Address = await _unmockablesWrapper.GetTokenEndPoint(disco);
            _tokenRequest.Scope = scope;
            var tokenResponse = await _unmockablesWrapper
                .RequestClientCredentialsTokenAsync(client, _tokenRequest);
            var accessToken = await _unmockablesWrapper.GetAccessToken(tokenResponse);
            client.SetBearerToken(accessToken);
            return client;
        }
    }
}
