using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HttpManager
{
    public class AccessTokenGetter : IAccessTokenGetter
    {
        private readonly IConfiguration _config;

        public AccessTokenGetter(IConfiguration config)
        {
            _config = config;
        }
        public async Task<HttpClient> GetToken(HttpClient client, string authUrl, string clientId, string clientSecret, string scopeKey)
        {
            var disco = await client.GetDiscoveryDocumentAsync(authUrl);
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = _config.GetSection(scopeKey).Value
            });
            client.SetBearerToken(tokenResponse.AccessToken);
            return client;
        }
    }
}
