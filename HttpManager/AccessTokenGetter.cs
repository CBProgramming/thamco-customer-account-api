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
        private readonly ClientCredentialsTokenRequest _tokenRequest;

        public AccessTokenGetter(IDiscoGetter discoGetter, ClientCredentialsTokenRequest tokenRequest)
        {
            _discoGetter = discoGetter;
            _tokenRequest = tokenRequest;
        }
        public async Task<HttpClient> GetToken(HttpClient client, string authUrl, string scope)
        {
            var disco = await client.GetDiscoveryDocumentAsync(authUrl);
            _tokenRequest.Address = await _discoGetter.GetTokenEndPoint(disco);
            _tokenRequest.Scope = scope;
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(_tokenRequest);
            client.SetBearerToken(tokenResponse.AccessToken);
            return client;
        }
    }
}
