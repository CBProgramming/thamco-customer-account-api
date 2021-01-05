using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HttpManager
{
    //Wrapper class to facilitate testing with static methods and readonly variables in external classes
    public interface IUnmockablesWrapper
    {
        Task<DiscoveryDocumentResponse> GetDiscoveryDocumentAsync(HttpClient client, string url);

        Task<TokenResponse> RequestClientCredentialsTokenAsync(HttpClient client, ClientCredentialsTokenRequest request);

        public Task<string> GetTokenEndPoint(DiscoveryDocumentResponse disco);

        public Task<string> GetAccessToken(TokenResponse tokenResponse);
    }
}
