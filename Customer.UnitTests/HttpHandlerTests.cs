using Customer.ReviewFacade;
using HttpManager;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Customer.UnitTests
{
    public class HttpHandlerTests
    {
        //public Mock<HttpClient> mockClient;
        public HttpClient httpClient;
        public Mock<IUnmockablesWrapper> mockWrapper;
        public Mock<IHttpClientFactory> mockFactory;
        public Mock<DiscoveryDocumentResponse> mockDiscoResponse;
        public Mock<TokenResponse> mockTokenResponse;
        public ClientCredentialsTokenRequest tokenRequest;
        public IReviewCustomerFacade facade;
        private IConfiguration config;
        private string urlKey = "url_key";
        private string urlValue = "url_value";
        private string clientKey = "client_key";
        private string scopeKey = "scope_key";
        private string scopeKeyValue = "scope_key_value";
        private string discoDocumentEndPoint = "disco_end_point";
        private string accessTokenValue = "access_token_value";
        private readonly string emptyString = "";
        private readonly string originalAddress = "";
        private readonly string originalScope = "";
        private readonly string clientId = "client_id";
        private readonly string clientSecret = "client_secret";
        private HttpHandler httpHandler;
        bool factoryReturnsNull = false;
        bool wrapperReturnsNullDisco = false;
        bool wrapperReturnsNullEndPoint = false;
        bool wrapperReturnsEmptyEndPoint = false;
        bool wrapperReturnsNullTokenResponse = false;
        bool wrapperReturnsNullAccessToken = false;
        bool wrapperReturnsEmptyAccessToken = false;

        private void SetupMockWrapper()
        {
            mockWrapper = new Mock<IUnmockablesWrapper>(MockBehavior.Strict);
            mockWrapper.Setup(w => w.GetDiscoveryDocumentAsync(It.IsAny<HttpClient>(),It.IsAny<string>()))
                .Returns(wrapperReturnsNullDisco?null:Task.FromResult(mockDiscoResponse.Object)).Verifiable();
            mockWrapper.Setup(w => w.RequestClientCredentialsTokenAsync(
                It.IsAny<HttpClient>(), It.IsAny<ClientCredentialsTokenRequest>()))
                .Returns(wrapperReturnsNullTokenResponse?null:Task.FromResult(mockTokenResponse.Object)).Verifiable();
            mockWrapper.Setup(w => w.GetTokenEndPoint(It.IsAny<DiscoveryDocumentResponse>()))
                .Returns(Task.FromResult(wrapperReturnsNullEndPoint?null:(wrapperReturnsEmptyEndPoint?"":discoDocumentEndPoint)));
            mockWrapper.Setup(w => w.GetAccessToken(It.IsAny<TokenResponse>()))
                .Returns(Task.FromResult(wrapperReturnsNullAccessToken?null:(wrapperReturnsEmptyAccessToken?"":accessTokenValue)));
        }

        private void SetupClientCredentialsTokenRequest()
        {
            tokenRequest = new ClientCredentialsTokenRequest
            {
                Address = originalAddress,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = originalScope
            };
        }

        private void SetupHttpFactoryMock(HttpClient client)
        {
            mockFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            mockFactory.Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(factoryReturnsNull ? null : client).Verifiable();
        }

        private void SetupConfig()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                { urlKey??"url_key", urlValue},
                { scopeKey??"scope_key", scopeKeyValue }
            };
            config = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }

        private void DefaultSetup()
        {
            httpClient = new HttpClient();
            SetupHttpFactoryMock(httpClient);
            SetupConfig();
            mockDiscoResponse = new Mock<DiscoveryDocumentResponse>(MockBehavior.Strict);
            SetupClientCredentialsTokenRequest();
            mockTokenResponse = new Mock<TokenResponse>(MockBehavior.Strict);
            SetupMockWrapper();
            httpHandler = new HttpHandler(mockFactory.Object, config, tokenRequest, mockWrapper.Object);
            SetupConfig();
        }

        [Fact]
        public async Task GetClient_ShouldReturnClient()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.NotNull(result);
            var objResult = result as HttpClient;
            Assert.NotNull(objResult);
            Assert.True(httpClient == result);
            mockFactory.Verify(f => f.CreateClient(clientKey), Times.Once);
            mockWrapper.Verify(w => w.GetDiscoveryDocumentAsync(httpClient, config.GetSection(urlKey).Value), Times.Once());
            Assert.Equal(tokenRequest.Address, discoDocumentEndPoint);
            Assert.Equal(tokenRequest.Scope, scopeKeyValue);
            Assert.Equal(tokenRequest.ClientId, clientId);
            Assert.Equal(tokenRequest.ClientSecret, clientSecret);
            mockWrapper.Verify(w => w.RequestClientCredentialsTokenAsync(httpClient, tokenRequest), Times.Once());
            mockWrapper.Verify(w => w.GetTokenEndPoint(mockDiscoResponse.Object), Times.Once());
            mockWrapper.Verify(w => w.GetAccessToken(mockTokenResponse.Object), Times.Once());
            Assert.Equal(httpClient.DefaultRequestHeaders.Authorization.Parameter, accessTokenValue);
        }

        [Fact]
        public async Task GetClient_NullClientFactory()
        {
            //Arrange
            DefaultSetup();
            httpHandler = new HttpHandler(null, config, tokenRequest, mockWrapper.Object);

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
            mockWrapper.Verify(w => w.GetDiscoveryDocumentAsync(It.IsAny<HttpClient>(), It.IsAny<string>()), Times.Never());
            Assert.Equal(tokenRequest.Address, originalAddress);
            Assert.Equal(tokenRequest.Scope, originalScope);
            Assert.Equal(tokenRequest.ClientId, clientId);
            Assert.Equal(tokenRequest.ClientSecret, clientSecret);
            mockWrapper.Verify(w => w.RequestClientCredentialsTokenAsync(It.IsAny<HttpClient>(),
                It.IsAny<ClientCredentialsTokenRequest>()), Times.Never());
            mockWrapper.Verify(w => w.GetTokenEndPoint(It.IsAny<DiscoveryDocumentResponse>()), Times.Never());
            mockWrapper.Verify(w => w.GetAccessToken(It.IsAny<TokenResponse>()), Times.Never());
            Assert.Null(httpClient.DefaultRequestHeaders.Authorization);
        }

        [Fact]
        public async Task GetClient_NullConfig()
        {
            //Arrange
            DefaultSetup();
            httpHandler = new HttpHandler(mockFactory.Object, null, tokenRequest, mockWrapper.Object);

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
            mockWrapper.Verify(w => w.GetDiscoveryDocumentAsync(It.IsAny<HttpClient>(), It.IsAny<string>()), Times.Never());
            Assert.Equal(tokenRequest.Address, originalAddress);
            Assert.Equal(tokenRequest.Scope, originalScope);
            Assert.Equal(tokenRequest.ClientId, clientId);
            Assert.Equal(tokenRequest.ClientSecret, clientSecret);
            mockWrapper.Verify(w => w.RequestClientCredentialsTokenAsync(It.IsAny<HttpClient>(),
                It.IsAny<ClientCredentialsTokenRequest>()), Times.Never());
            mockWrapper.Verify(w => w.GetTokenEndPoint(It.IsAny<DiscoveryDocumentResponse>()), Times.Never());
            mockWrapper.Verify(w => w.GetAccessToken(It.IsAny<TokenResponse>()), Times.Never());
            Assert.Null(httpClient.DefaultRequestHeaders.Authorization);
        }

        [Fact]
        public async Task GetClient_NullTokenRequest()
        {
            //Arrange
            DefaultSetup();
            httpHandler = new HttpHandler(mockFactory.Object, config, null, mockWrapper.Object);

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
            mockWrapper.Verify(w => w.GetDiscoveryDocumentAsync(It.IsAny<HttpClient>(), It.IsAny<string>()), Times.Never());
            Assert.Equal(tokenRequest.Address, originalAddress);
            Assert.Equal(tokenRequest.Scope, originalScope);
            Assert.Equal(tokenRequest.ClientId, clientId);
            Assert.Equal(tokenRequest.ClientSecret, clientSecret);
            mockWrapper.Verify(w => w.RequestClientCredentialsTokenAsync(It.IsAny<HttpClient>(),
                It.IsAny<ClientCredentialsTokenRequest>()), Times.Never());
            mockWrapper.Verify(w => w.GetTokenEndPoint(It.IsAny<DiscoveryDocumentResponse>()), Times.Never());
            mockWrapper.Verify(w => w.GetAccessToken(It.IsAny<TokenResponse>()), Times.Never());
            Assert.Null(httpClient.DefaultRequestHeaders.Authorization);
        }

        [Fact]
        public async Task GetClient_NullWrapper()
        {
            //Arrange
            DefaultSetup();
            httpHandler = new HttpHandler(mockFactory.Object, config, tokenRequest, null);

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
            mockWrapper.Verify(w => w.GetDiscoveryDocumentAsync(It.IsAny<HttpClient>(), It.IsAny<string>()), Times.Never());
            Assert.Equal(tokenRequest.Address, originalAddress);
            Assert.Equal(tokenRequest.Scope, originalScope);
            Assert.Equal(tokenRequest.ClientId, clientId);
            Assert.Equal(tokenRequest.ClientSecret, clientSecret);
            mockWrapper.Verify(w => w.RequestClientCredentialsTokenAsync(It.IsAny<HttpClient>(),
                It.IsAny<ClientCredentialsTokenRequest>()), Times.Never());
            mockWrapper.Verify(w => w.GetTokenEndPoint(It.IsAny<DiscoveryDocumentResponse>()), Times.Never());
            mockWrapper.Verify(w => w.GetAccessToken(It.IsAny<TokenResponse>()), Times.Never());
            Assert.Null(httpClient.DefaultRequestHeaders.Authorization);
        }

        [Fact]
        public async Task NewCustomer_FactoryReturnsNull()
        {
            //Arrange
            factoryReturnsNull = true;
            DefaultSetup();

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(f => f.CreateClient(clientKey), Times.Once);
            mockWrapper.Verify(w => w.GetDiscoveryDocumentAsync(It.IsAny<HttpClient>(), It.IsAny<string>()), Times.Never());
            Assert.Equal(tokenRequest.Address, originalAddress);
            Assert.Equal(tokenRequest.Scope, originalScope);
            Assert.Equal(tokenRequest.ClientId, clientId);
            Assert.Equal(tokenRequest.ClientSecret, clientSecret);
            mockWrapper.Verify(w => w.RequestClientCredentialsTokenAsync(It.IsAny<HttpClient>(), 
                It.IsAny<ClientCredentialsTokenRequest>()), Times.Never());
            mockWrapper.Verify(w => w.GetTokenEndPoint(It.IsAny<DiscoveryDocumentResponse>()), Times.Never());
            mockWrapper.Verify(w => w.GetAccessToken(It.IsAny<TokenResponse>()), Times.Never());
            Assert.Null(httpClient.DefaultRequestHeaders.Authorization);
        }

        [Fact]
        public async Task NewCustomer_WrapperReturnsNullDisco()
        {
            //Arrange
            wrapperReturnsNullDisco = true;
            DefaultSetup();

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(f => f.CreateClient(clientKey), Times.Once);
            mockWrapper.Verify(w => w.GetDiscoveryDocumentAsync(httpClient, config.GetSection(urlKey).Value), Times.Once());
            Assert.Equal(tokenRequest.Address, originalAddress);
            Assert.Equal(tokenRequest.Scope, originalScope);
            Assert.Equal(tokenRequest.ClientId, clientId);
            Assert.Equal(tokenRequest.ClientSecret, clientSecret);
            mockWrapper.Verify(w => w.RequestClientCredentialsTokenAsync(It.IsAny<HttpClient>(),
                It.IsAny<ClientCredentialsTokenRequest>()), Times.Never());
            mockWrapper.Verify(w => w.GetTokenEndPoint(It.IsAny<DiscoveryDocumentResponse>()), Times.Never());
            mockWrapper.Verify(w => w.GetAccessToken(It.IsAny<TokenResponse>()), Times.Never());
            Assert.Null(httpClient.DefaultRequestHeaders.Authorization);
        }

        [Fact]
        public async Task GetClient_WrapperReturnsNullEndPoint()
        {
            //Arrange
            wrapperReturnsNullEndPoint = true;
            DefaultSetup();

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(f => f.CreateClient(clientKey), Times.Once);
            mockWrapper.Verify(w => w.GetDiscoveryDocumentAsync(httpClient, config.GetSection(urlKey).Value), Times.Once());
            mockWrapper.Verify(w => w.GetTokenEndPoint(mockDiscoResponse.Object), Times.Once());
            Assert.Equal(tokenRequest.Address, originalAddress);
            Assert.Equal(tokenRequest.Scope, originalScope);
            Assert.Equal(tokenRequest.ClientId, clientId);
            Assert.Equal(tokenRequest.ClientSecret, clientSecret);
            mockWrapper.Verify(w => w.RequestClientCredentialsTokenAsync(httpClient, tokenRequest), Times.Never());
            mockWrapper.Verify(w => w.GetAccessToken(It.IsAny<TokenResponse>()), Times.Never());
            Assert.Null(httpClient.DefaultRequestHeaders.Authorization);
        }

        [Fact]
        public async Task GetClient_WrapperReturnsEmptyEndPoint()
        {
            //Arrange
            wrapperReturnsEmptyEndPoint = true;
            DefaultSetup();

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(f => f.CreateClient(clientKey), Times.Once);
            mockWrapper.Verify(w => w.GetDiscoveryDocumentAsync(httpClient, config.GetSection(urlKey).Value), Times.Once());
            mockWrapper.Verify(w => w.GetTokenEndPoint(mockDiscoResponse.Object), Times.Once());
            Assert.Equal(tokenRequest.Address, originalAddress);
            Assert.Equal(tokenRequest.Scope, originalScope);
            Assert.Equal(tokenRequest.ClientId, clientId);
            Assert.Equal(tokenRequest.ClientSecret, clientSecret);
            mockWrapper.Verify(w => w.RequestClientCredentialsTokenAsync(httpClient, tokenRequest), Times.Never());
            mockWrapper.Verify(w => w.GetAccessToken(It.IsAny<TokenResponse>()), Times.Never());
            Assert.Null(httpClient.DefaultRequestHeaders.Authorization);
        }

        [Fact]
        public async Task GetClient_WrapperReturnsNullTokenResponse()
        {
            //Arrange
            wrapperReturnsNullTokenResponse = true;
            DefaultSetup();

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(f => f.CreateClient(clientKey), Times.Once);
            mockWrapper.Verify(w => w.GetDiscoveryDocumentAsync(httpClient, config.GetSection(urlKey).Value), Times.Once());
            mockWrapper.Verify(w => w.GetTokenEndPoint(mockDiscoResponse.Object), Times.Once());
            Assert.Equal(tokenRequest.Address, discoDocumentEndPoint);
            Assert.Equal(tokenRequest.Scope, scopeKeyValue);
            Assert.Equal(tokenRequest.ClientId, clientId);
            Assert.Equal(tokenRequest.ClientSecret, clientSecret);
            mockWrapper.Verify(w => w.RequestClientCredentialsTokenAsync(httpClient, tokenRequest), Times.Once());
            mockWrapper.Verify(w => w.GetAccessToken(It.IsAny<TokenResponse>()), Times.Never());
            Assert.Null(httpClient.DefaultRequestHeaders.Authorization);
        }

        [Fact]
        public async Task GetClient_WrapperReturnsNullAccessToken()
        {
            //Arrange
            wrapperReturnsNullAccessToken = true;
            DefaultSetup();

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(f => f.CreateClient(clientKey), Times.Once);
            mockWrapper.Verify(w => w.GetDiscoveryDocumentAsync(httpClient, config.GetSection(urlKey).Value), Times.Once());
            mockWrapper.Verify(w => w.GetTokenEndPoint(mockDiscoResponse.Object), Times.Once());
            Assert.Equal(tokenRequest.Address, discoDocumentEndPoint);
            Assert.Equal(tokenRequest.Scope, scopeKeyValue);
            Assert.Equal(tokenRequest.ClientId, clientId);
            Assert.Equal(tokenRequest.ClientSecret, clientSecret);
            mockWrapper.Verify(w => w.RequestClientCredentialsTokenAsync(httpClient, tokenRequest), Times.Once());
            mockWrapper.Verify(w => w.GetAccessToken(mockTokenResponse.Object), Times.Once());
            Assert.Null(httpClient.DefaultRequestHeaders.Authorization);
        }

        [Fact]
        public async Task GetClient_WrapperReturnsEmptyAccessToken()
        {
            //Arrange
            wrapperReturnsEmptyAccessToken = true;
            DefaultSetup();

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(f => f.CreateClient(clientKey), Times.Once);
            mockWrapper.Verify(w => w.GetDiscoveryDocumentAsync(httpClient, config.GetSection(urlKey).Value), Times.Once());
            mockWrapper.Verify(w => w.GetTokenEndPoint(mockDiscoResponse.Object), Times.Once());
            Assert.Equal(tokenRequest.Address, discoDocumentEndPoint);
            Assert.Equal(tokenRequest.Scope, scopeKeyValue);
            Assert.Equal(tokenRequest.ClientId, clientId);
            Assert.Equal(tokenRequest.ClientSecret, clientSecret);
            mockWrapper.Verify(w => w.RequestClientCredentialsTokenAsync(httpClient, tokenRequest), Times.Once());
            mockWrapper.Verify(w => w.GetAccessToken(mockTokenResponse.Object), Times.Once());
            Assert.Null(httpClient.DefaultRequestHeaders.Authorization);
        }

        [Fact]
        public async Task GetClient_NullUrlKey()
        {
            //Arrange
            urlKey = null;
            DefaultSetup();

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
            mockWrapper.Verify(w => w.GetDiscoveryDocumentAsync(It.IsAny<HttpClient>(), It.IsAny<string>()), Times.Never());
            Assert.Equal(tokenRequest.Address, originalAddress);
            Assert.Equal(tokenRequest.Scope, originalScope);
            Assert.Equal(tokenRequest.ClientId, clientId);
            Assert.Equal(tokenRequest.ClientSecret, clientSecret);
            mockWrapper.Verify(w => w.RequestClientCredentialsTokenAsync(It.IsAny<HttpClient>(),
                It.IsAny<ClientCredentialsTokenRequest>()), Times.Never());
            mockWrapper.Verify(w => w.GetTokenEndPoint(It.IsAny<DiscoveryDocumentResponse>()), Times.Never());
            mockWrapper.Verify(w => w.GetAccessToken(It.IsAny<TokenResponse>()), Times.Never());
            Assert.Null(httpClient.DefaultRequestHeaders.Authorization);
        }

        [Fact]
        public async Task GetClient_EmptyUrlKey()
        {
            //Arrange
            urlKey = "";
            DefaultSetup();

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
            mockWrapper.Verify(w => w.GetDiscoveryDocumentAsync(It.IsAny<HttpClient>(), It.IsAny<string>()), Times.Never());
            Assert.Equal(tokenRequest.Address, originalAddress);
            Assert.Equal(tokenRequest.Scope, originalScope);
            Assert.Equal(tokenRequest.ClientId, clientId);
            Assert.Equal(tokenRequest.ClientSecret, clientSecret);
            mockWrapper.Verify(w => w.RequestClientCredentialsTokenAsync(It.IsAny<HttpClient>(),
                It.IsAny<ClientCredentialsTokenRequest>()), Times.Never());
            mockWrapper.Verify(w => w.GetTokenEndPoint(It.IsAny<DiscoveryDocumentResponse>()), Times.Never());
            mockWrapper.Verify(w => w.GetAccessToken(It.IsAny<TokenResponse>()), Times.Never());
            Assert.Null(httpClient.DefaultRequestHeaders.Authorization);
        }

        [Fact]
        public async Task GetClient_NullClientKey()
        {
            //Arrange
            clientKey = null;
            DefaultSetup();

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
            mockWrapper.Verify(w => w.GetDiscoveryDocumentAsync(It.IsAny<HttpClient>(), It.IsAny<string>()), Times.Never());
            Assert.Equal(tokenRequest.Address, originalAddress);
            Assert.Equal(tokenRequest.Scope, originalScope);
            Assert.Equal(tokenRequest.ClientId, clientId);
            Assert.Equal(tokenRequest.ClientSecret, clientSecret);
            mockWrapper.Verify(w => w.RequestClientCredentialsTokenAsync(It.IsAny<HttpClient>(),
                It.IsAny<ClientCredentialsTokenRequest>()), Times.Never());
            mockWrapper.Verify(w => w.GetTokenEndPoint(It.IsAny<DiscoveryDocumentResponse>()), Times.Never());
            mockWrapper.Verify(w => w.GetAccessToken(It.IsAny<TokenResponse>()), Times.Never());
            Assert.Null(httpClient.DefaultRequestHeaders.Authorization);
        }

        [Fact]
        public async Task GetClient_EmptyClientKey()
        {
            //Arrange
            clientKey = "";
            DefaultSetup();

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
            mockWrapper.Verify(w => w.GetDiscoveryDocumentAsync(It.IsAny<HttpClient>(), It.IsAny<string>()), Times.Never());
            Assert.Equal(tokenRequest.Address, originalAddress);
            Assert.Equal(tokenRequest.Scope, originalScope);
            Assert.Equal(tokenRequest.ClientId, clientId);
            Assert.Equal(tokenRequest.ClientSecret, clientSecret);
            mockWrapper.Verify(w => w.RequestClientCredentialsTokenAsync(It.IsAny<HttpClient>(),
                It.IsAny<ClientCredentialsTokenRequest>()), Times.Never());
            mockWrapper.Verify(w => w.GetTokenEndPoint(It.IsAny<DiscoveryDocumentResponse>()), Times.Never());
            mockWrapper.Verify(w => w.GetAccessToken(It.IsAny<TokenResponse>()), Times.Never());
            Assert.Null(httpClient.DefaultRequestHeaders.Authorization);
        }

        [Fact]
        public async Task GetClient_NullScopeKey()
        {
            //Arrange
            scopeKey = null;
            DefaultSetup();

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
            mockWrapper.Verify(w => w.GetDiscoveryDocumentAsync(It.IsAny<HttpClient>(), It.IsAny<string>()), Times.Never());
            Assert.Equal(tokenRequest.Address, originalAddress);
            Assert.Equal(tokenRequest.Scope, originalScope);
            Assert.Equal(tokenRequest.ClientId, clientId);
            Assert.Equal(tokenRequest.ClientSecret, clientSecret);
            mockWrapper.Verify(w => w.RequestClientCredentialsTokenAsync(It.IsAny<HttpClient>(),
                It.IsAny<ClientCredentialsTokenRequest>()), Times.Never());
            mockWrapper.Verify(w => w.GetTokenEndPoint(It.IsAny<DiscoveryDocumentResponse>()), Times.Never());
            mockWrapper.Verify(w => w.GetAccessToken(It.IsAny<TokenResponse>()), Times.Never());
            Assert.Null(httpClient.DefaultRequestHeaders.Authorization);
        }

        [Fact]
        public async Task GetClient_EmptyScopeKey()
        {
            //Arrange
            scopeKey = "";
            DefaultSetup();

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
            mockWrapper.Verify(w => w.GetDiscoveryDocumentAsync(It.IsAny<HttpClient>(), It.IsAny<string>()), Times.Never());
            Assert.Equal(tokenRequest.Address, originalAddress);
            Assert.Equal(tokenRequest.Scope, originalScope);
            Assert.Equal(tokenRequest.ClientId, clientId);
            Assert.Equal(tokenRequest.ClientSecret, clientSecret);
            mockWrapper.Verify(w => w.RequestClientCredentialsTokenAsync(It.IsAny<HttpClient>(),
                It.IsAny<ClientCredentialsTokenRequest>()), Times.Never());
            mockWrapper.Verify(w => w.GetTokenEndPoint(It.IsAny<DiscoveryDocumentResponse>()), Times.Never());
            mockWrapper.Verify(w => w.GetAccessToken(It.IsAny<TokenResponse>()), Times.Never());
            Assert.Null(httpClient.DefaultRequestHeaders.Authorization);
        }

        [Fact]
        public async Task GetClient_NullAuthServerUrlValue()
        {
            //Arrange
            urlValue = null;
            DefaultSetup();

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
            mockWrapper.Verify(w => w.GetDiscoveryDocumentAsync(It.IsAny<HttpClient>(), It.IsAny<string>()), Times.Never());
            Assert.Equal(tokenRequest.Address, originalAddress);
            Assert.Equal(tokenRequest.Scope, originalScope);
            Assert.Equal(tokenRequest.ClientId, clientId);
            Assert.Equal(tokenRequest.ClientSecret, clientSecret);
            mockWrapper.Verify(w => w.RequestClientCredentialsTokenAsync(It.IsAny<HttpClient>(),
                It.IsAny<ClientCredentialsTokenRequest>()), Times.Never());
            mockWrapper.Verify(w => w.GetTokenEndPoint(It.IsAny<DiscoveryDocumentResponse>()), Times.Never());
            mockWrapper.Verify(w => w.GetAccessToken(It.IsAny<TokenResponse>()), Times.Never());
            Assert.Null(httpClient.DefaultRequestHeaders.Authorization);
        }

        [Fact]
        public async Task GetClient_EmptyAuthServerUrlValue()
        {
            //Arrange
            urlValue = "";
            DefaultSetup();

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
            mockWrapper.Verify(w => w.GetDiscoveryDocumentAsync(It.IsAny<HttpClient>(), It.IsAny<string>()), Times.Never());
            Assert.Equal(tokenRequest.Address, originalAddress);
            Assert.Equal(tokenRequest.Scope, originalScope);
            Assert.Equal(tokenRequest.ClientId, clientId);
            Assert.Equal(tokenRequest.ClientSecret, clientSecret);
            mockWrapper.Verify(w => w.RequestClientCredentialsTokenAsync(It.IsAny<HttpClient>(),
                It.IsAny<ClientCredentialsTokenRequest>()), Times.Never());
            mockWrapper.Verify(w => w.GetTokenEndPoint(It.IsAny<DiscoveryDocumentResponse>()), Times.Never());
            mockWrapper.Verify(w => w.GetAccessToken(It.IsAny<TokenResponse>()), Times.Never());
            Assert.Null(httpClient.DefaultRequestHeaders.Authorization);
        }

        [Fact]
        public async Task GetClient_NullScopeKeyValue()
        {
            //Arrange
            scopeKeyValue = null;
            DefaultSetup();

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
            mockWrapper.Verify(w => w.GetDiscoveryDocumentAsync(It.IsAny<HttpClient>(), It.IsAny<string>()), Times.Never());
            Assert.Equal(tokenRequest.Address, originalAddress);
            Assert.Equal(tokenRequest.Scope, originalScope);
            Assert.Equal(tokenRequest.ClientId, clientId);
            Assert.Equal(tokenRequest.ClientSecret, clientSecret);
            mockWrapper.Verify(w => w.RequestClientCredentialsTokenAsync(It.IsAny<HttpClient>(),
                It.IsAny<ClientCredentialsTokenRequest>()), Times.Never());
            mockWrapper.Verify(w => w.GetTokenEndPoint(It.IsAny<DiscoveryDocumentResponse>()), Times.Never());
            mockWrapper.Verify(w => w.GetAccessToken(It.IsAny<TokenResponse>()), Times.Never());
            Assert.Null(httpClient.DefaultRequestHeaders.Authorization);
        }

        [Fact]
        public async Task GetClient_EmptyScopeKeyValue()
        {
            //Arrange
            scopeKeyValue = "";
            DefaultSetup();

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
            mockWrapper.Verify(w => w.GetDiscoveryDocumentAsync(It.IsAny<HttpClient>(), It.IsAny<string>()), Times.Never());
            Assert.Equal(tokenRequest.Address, originalAddress);
            Assert.Equal(tokenRequest.Scope, originalScope);
            Assert.Equal(tokenRequest.ClientId, clientId);
            Assert.Equal(tokenRequest.ClientSecret, clientSecret);
            mockWrapper.Verify(w => w.RequestClientCredentialsTokenAsync(It.IsAny<HttpClient>(),
                It.IsAny<ClientCredentialsTokenRequest>()), Times.Never());
            mockWrapper.Verify(w => w.GetTokenEndPoint(It.IsAny<DiscoveryDocumentResponse>()), Times.Never());
            mockWrapper.Verify(w => w.GetAccessToken(It.IsAny<TokenResponse>()), Times.Never());
            Assert.Null(httpClient.DefaultRequestHeaders.Authorization);
        }
    }
}
