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
        private readonly IDiscoGetter _discoGetter;

        public AccessTokenGetter(IDiscoGetter discoGetter)
        {
            _discoGetter = discoGetter;
        }
        public async Task<HttpClient> GetToken(HttpClient client, string authUrl, string clientId, string clientSecret, string scope)
        {
            var disco = await client.GetDiscoveryDocumentAsync(authUrl);
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = await _discoGetter.GetTokenEndPoint(disco),
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = scope
            });
            client.SetBearerToken(tokenResponse.AccessToken);
            return client;
        }
    }
}
