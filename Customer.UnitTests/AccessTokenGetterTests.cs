using AutoMapper.Configuration;
using HttpManager;
using IdentityModel.Client;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Customer.UnitTests
{
    public class AccessTokenGetterTests
    {
        public HttpClient client;
        public Mock<HttpClient> mockClient;
        public Mock<DiscoveryDocumentResponse> mockDiscoResponse;
        //public Mock<HttpMessageHandler> mockHandler;
        //private IConfiguration config;
        private Mock<IDiscoGetter> mockDiscoGetter;
        private string endPointAddress = "endPointAddress";
        private string authUrl = "https://authurl";
        private string clientId = "client_id";
        private string clientSecret = "client_secret";
        private string scope = "scope";
        private IAccessTokenGetter accessTokenGetter;


        /*private void SetMockMessageHandler(HttpResponseMessage expected)
        {
            mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(expected)
                .Verifiable();
        }*/

        /*private void SetupRealHttpClient()
        {
            client = new HttpClient(mockHandler.Object);
            client.BaseAddress = new Uri("http://test");

        }*/



        /*        private void SetupConfig(string custUri = null, string custAuthUrlKey = null, string? custReviewAPIKey = null,
                    string? custReviewScope = null)
                {
                    var myConfiguration = new Dictionary<string, string>
                    {
                        {"ClientId", "customer_account_api"},
                        {"ClientSecret", "not_a_real_password"},
                        {"CustomerAuthServerUrl", "https://localhost:43389"},
                        {"StaffAuthServerUrl", "https://localhost:43390"},
                        {"CustomerOrderingUrl", "https://localhost:50836"},
                        {"ReviewUrl", "https://localhost:50736"},
                        {"ReviewUri", custUri??reviewUriValue},
                        {"AuthUri", "/api/users"},
                        {"CustomerAuthCustomerScope", "customer_auth_customer_api"},
                        {"CustomerOrderingScope", "customer_ordering_api"},
                        {"ReviewScope", "review_api"},
                        {"CustomerAuthServerUrlKey", custAuthUrlKey??customerAuthServerUrlKeyValue},
                        {"CustomerReviewAPIKey", custReviewAPIKey??customerReviewApiKeyValue},
                        {"CustomerReviewScopeKey", custReviewScope??customerReviewScopeKeyValue}
                    };
                    config = new ConfigurationBuilder()
                        .AddInMemoryCollection(myConfiguration)
                        .Build();
                }*/

        /*private void SetupHttpHandlerMock()
        {
            mockHttpHandler = new Mock<IHttpHandler>(MockBehavior.Strict);
            mockHttpHandler.Setup(f => f.GetClient(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(client)).Verifiable();
        }*/
        
        private void SetupMockHttpClient()
        {
            mockClient = new Mock<HttpClient>(MockBehavior.Strict);
            /*mockClient.Setup(c => c.GetDiscoveryDocumentAsync(authUrl))
                .Returns(Task.FromResult(mockDiscoResponse.Object)).Verifiable();*/

        }

        private void SetupMockDiscoveryDocumentResponse()
        {
            mockDiscoResponse = new Mock<DiscoveryDocumentResponse>(MockBehavior.Strict);
        }

        private void SetupDiscoGetterMock()
        {
            mockDiscoGetter = new Mock<IDiscoGetter>(MockBehavior.Strict);
            mockDiscoGetter.Setup(f => f.GetTokenEndPoint(It.IsAny<DiscoveryDocumentResponse>()))
                .Returns(Task.FromResult(endPointAddress)).Verifiable();
        }

/*        private void DefaultSetup()
        {
            *//*var expectedResult = new HttpResponseMessage
            {
                StatusCode = statusCode
            };*//*
            //SetMockMessageHandler(expectedResult);
            SetupMockHttpClient();
            SetupDiscoGetterMock();
            accessTokenGetter = new AccessTokenGetter(mockDiscoGetter.Object);
        }*/
    }
}
