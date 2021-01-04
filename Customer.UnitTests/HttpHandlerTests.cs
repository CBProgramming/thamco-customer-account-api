using Customer.ReviewFacade;
using HttpManager;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Customer.UnitTests
{
    public class HttpHandlerTests
    {
        public Mock<HttpClient> client;
        public Mock<IHttpClientFactory> mockFactory;
        public IReviewCustomerFacade facade;
        private IConfiguration config;
        private string urlKey = "url_key";
        private string urlValue = "url_value";
        private string clientKey = "client_key";
        private string scopeKey = "scope_key";
        private string scopeKeyValue = "scope_key_value";
        private HttpHandler httpHandler;
        private Mock<IAccessTokenGetter> mockTokenGetter;
        bool tokenGetterReturnsNull = false;
        bool factoryReturnsNull = false;

        private void SetupRealHttpClient()
        {
            client = new Mock<HttpClient>();
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

        private void SetupMockTokenGetter()
        {
            mockTokenGetter = new Mock<IAccessTokenGetter>(MockBehavior.Strict);
            mockTokenGetter.Setup(f => f.GetToken(It.IsAny<HttpClient>(), It.IsAny<string>(), 
                It.IsAny<string>()))
                .Returns(Task.FromResult(tokenGetterReturnsNull?null:client.Object)).Verifiable();
        }

        private void DefaultSetup()
        {
            //SetMockMessageHandler(expectedResult);
            SetupRealHttpClient();
            SetupHttpFactoryMock(client.Object);
            SetupConfig();
            SetupMockTokenGetter();
            httpHandler = new HttpHandler(mockFactory.Object, config, mockTokenGetter.Object);
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
            Assert.True(client.Object == result);
            mockFactory.Verify(factory => factory.CreateClient(clientKey), Times.Once);
            mockTokenGetter.Verify(t => t.GetToken(client.Object, urlValue, scopeKeyValue), Times.Once);
        }

        [Fact]
        public async Task NewCustomer_TokenGetterReturnsNull()
        {
            //Arrange
            tokenGetterReturnsNull = true;
            DefaultSetup();

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(factory => factory.CreateClient(clientKey), Times.Once);
            mockTokenGetter.Verify(t => t.GetToken(client.Object, urlValue, scopeKeyValue), Times.Once);
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
            mockFactory.Verify(factory => factory.CreateClient(clientKey), Times.Once);
            mockTokenGetter.Verify(t => t.GetToken(It.IsAny<HttpClient>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task NewCustomer_NullUrlKey()
        {
            //Arrange
            urlKey = null;
            DefaultSetup();

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(factory => factory.CreateClient(It.IsAny<string>()), Times.Never);
            mockTokenGetter.Verify(t => t.GetToken(It.IsAny<HttpClient>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task NewCustomer_EmptyUrlKey()
        {
            //Arrange
            urlKey = "";
            DefaultSetup();

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(factory => factory.CreateClient(It.IsAny<string>()), Times.Never);
            mockTokenGetter.Verify(t => t.GetToken(It.IsAny<HttpClient>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task NewCustomer_NullClientKey()
        {
            //Arrange
            clientKey = null;
            DefaultSetup();

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(factory => factory.CreateClient(It.IsAny<string>()), Times.Never);
            mockTokenGetter.Verify(t => t.GetToken(It.IsAny<HttpClient>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task NewCustomer_EmptyClientKey()
        {
            //Arrange
            clientKey = "";
            DefaultSetup();

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(factory => factory.CreateClient(It.IsAny<string>()), Times.Never);
            mockTokenGetter.Verify(t => t.GetToken(It.IsAny<HttpClient>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task NewCustomer_NullScopeKey()
        {
            //Arrange
            scopeKey = null;
            DefaultSetup();

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(factory => factory.CreateClient(It.IsAny<string>()), Times.Never);
            mockTokenGetter.Verify(t => t.GetToken(It.IsAny<HttpClient>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task NewCustomer_EmptyScopeKey()
        {
            //Arrange
            scopeKey = "";
            DefaultSetup();

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(factory => factory.CreateClient(It.IsAny<string>()), Times.Never);
            mockTokenGetter.Verify(t => t.GetToken(It.IsAny<HttpClient>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task NewCustomer_NullAuthServerUrlValue()
        {
            //Arrange
            urlValue = null;
            DefaultSetup();

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(factory => factory.CreateClient(It.IsAny<string>()), Times.Never);
            mockTokenGetter.Verify(t => t.GetToken(It.IsAny<HttpClient>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task NewCustomer_EmptyAuthServerUrlValue()
        {
            //Arrange
            urlValue = "";
            DefaultSetup();

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(factory => factory.CreateClient(It.IsAny<string>()), Times.Never);
            mockTokenGetter.Verify(t => t.GetToken(It.IsAny<HttpClient>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task NewCustomer_NullScopeKeyValue()
        {
            //Arrange
            scopeKeyValue = null;
            DefaultSetup();

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(factory => factory.CreateClient(It.IsAny<string>()), Times.Never);
            mockTokenGetter.Verify(t => t.GetToken(It.IsAny<HttpClient>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task NewCustomer_EmptyScopeKeyValue()
        {
            //Arrange
            scopeKeyValue = "";
            DefaultSetup();

            //Act
            var result = await httpHandler.GetClient(urlKey, clientKey, scopeKey);

            //Assert
            Assert.Null(result);
            mockFactory.Verify(factory => factory.CreateClient(It.IsAny<string>()), Times.Never);
            mockTokenGetter.Verify(t => t.GetToken(It.IsAny<HttpClient>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
