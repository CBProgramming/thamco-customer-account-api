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
            if (!InitialVariablesOk(urlKey, scopeKey, clientKey))
            {
                return null;
            }
            string authServerUrl = _config.GetSection(urlKey).Value;
            string scope = _config.GetSection(scopeKey).Value;
            if (string.IsNullOrEmpty(scope) || string.IsNullOrEmpty(authServerUrl))
            {
                return null;
            }
            HttpClient client = _httpClientFactory.CreateClient(clientKey);
            if (client == null)
            {
                return null;
            }
            var disco = await GetDisco(client, authServerUrl);
            if (disco == null)
            {
                return null;
            }
            var tokenEndPoint = await _unmockablesWrapper.GetTokenEndPoint(disco);
            if (string.IsNullOrEmpty(tokenEndPoint))
            {
                return null;
            }
            _tokenRequest.Address = tokenEndPoint;
            _tokenRequest.Scope = scope;
            var tokenResponse = await GetTokenResponse(client, _tokenRequest);
            if (tokenResponse == null)
            {
                return null;
            }
            var accessToken = await _unmockablesWrapper.GetAccessToken(tokenResponse);
            if (string.IsNullOrEmpty(accessToken))
            {
                return null;
            }
            client.SetBearerToken(accessToken);
            return client;
        }

        private bool InitialVariablesOk(string urlKey, string scopeKey, string clientKey)
        {
            return _httpClientFactory != null
                && _config != null
                && _tokenRequest != null
                && _unmockablesWrapper != null
                && !string.IsNullOrEmpty(urlKey)
                && !string.IsNullOrEmpty(scopeKey)
                && !string.IsNullOrEmpty(clientKey);
        }

        private async Task<DiscoveryDocumentResponse> GetDisco(HttpClient client, string authServerUrl)
        {
            DiscoveryDocumentResponse disco;
            try
            {
                return await _unmockablesWrapper.GetDiscoveryDocumentAsync(client, authServerUrl);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private async Task<TokenResponse> GetTokenResponse(HttpClient client, 
            ClientCredentialsTokenRequest tokenRequest)
        {
            TokenResponse tokenResponse;
            try
            {
                return await _unmockablesWrapper
                .RequestClientCredentialsTokenAsync(client, _tokenRequest);
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
