using Customer.OrderFacade;
using Customer.OrderFacade.Models;
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
    public class OrderFacadeTests
    {
        public OrderingCustomerDto customer;
        public HttpClient client;
        public Mock<IHttpClientFactory> mockFactory;
        public Mock<HttpClient> mockClient;
        public Mock<HttpMessageHandler> mockHandler;
        public IOrderFacade facade;
        private IConfiguration config;
        private Mock<IHttpHandler> mockHttpHandler;
        private string customerUriValue = "/api/Customer";
        private string customerAuthServerUrlKeyValue = "CustomerAuthServerUrl";
        private string customerOrderingApiKeyValue = "CustomerOrderingAPI";
        private string customerOrderingScopeKeyValue = "CustomerOrderingScope";

        private void SetupCustomer()
        {
            customer = new OrderingCustomerDto
            {
                CustomerId = 1,
                GivenName = "Fake",
                FamilyName = "Name",
                AddressOne = "Address 1",
                AddressTwo = "Address 2",
                Town = "Town",
                State = "State",
                AreaCode = "Area Code",
                Country = "Country",
                EmailAddress = "email@email.com",
                TelephoneNumber = "07123456789",
                CanPurchase = true,
                Active = true
            };
        }

        private void SetMockMessageHandler(HttpResponseMessage expected)
        {
            mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(expected)
                .Verifiable();
        }

        private void SetupRealHttpClient(HttpResponseMessage expected)
        {
            client = new HttpClient(mockHandler.Object);
            client.BaseAddress = new Uri("http://test");

        }

        private void SetupHttpFactoryMock(HttpClient client)
        {
            mockFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            mockFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client).Verifiable();
        }

        private void SetupConfig(string custUri = null, string custAuthUrlKey = null, string? custOrderingAPIKey = null, 
            string? custOrderingScope = null)
        {
            var myConfiguration = new Dictionary<string, string>
            {
                {"ClientId", "customer_account_api"},
                {"ClientSecret", "not_a_real_password"},
                {"CustomerAuthServerUrl", "https://localhost:43389"},
                {"StaffAuthServerUrl", "https://localhost:43390"},
                {"CustomerOrderingUrl", "https://localhost:50836"},
                {"CustomerUri", custUri??customerUriValue},
                {"ReviewUrl", "https://localhost:50736"},
                {"ReviewUri", "/api/CustomerAccount"},
                {"AuthUri", "/api/users"},
                {"CustomerAuthCustomerScope", "customer_auth_customer_api"},
                {"CustomerOrderingScope", "customer_ordering_api"},
                {"ReviewScope", "review_api"},
                {"CustomerAuthServerUrlKey", custAuthUrlKey??customerAuthServerUrlKeyValue},
                {"CustomerOrderingAPIKey", custOrderingAPIKey??customerOrderingApiKeyValue},
                {"CustomerOrderingScopeKey", custOrderingScope??customerOrderingScopeKeyValue}
            };
            config = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }

        private void SetupHttpHandlerMock()
        {
            mockHttpHandler = new Mock<IHttpHandler>(MockBehavior.Strict);
            mockHttpHandler.Setup(f => f.GetClient(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(client)).Verifiable();
        }

        private void DefaultSetupRealHttpClient(HttpStatusCode statusCode)
        {
            SetupCustomer();
            var expectedResult = new HttpResponseMessage
            {
                StatusCode = statusCode
            };
            SetMockMessageHandler(expectedResult);
            SetupRealHttpClient(expectedResult);
            SetupHttpFactoryMock(client);
            SetupConfig();
            SetupHttpHandlerMock();
            facade = new OrderFacade.OrderFacade(config, mockHttpHandler.Object);
            SetupConfig();
        }

        [Fact]
        public async Task NewCustomer_OKResult_ShouldReturnTrue()
        {
            //Arrange
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/api/Customer/");

            //Act
            var result = await facade.NewCustomer(customer);

            //Assert
            Assert.True(true == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Once(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post && req.RequestUri == expectedUri), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI",
                "CustomerOrderingScope"), Times.Once);
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI", 
                "CustomerOrderingScope"), Times.Once);
        }

        [Fact]
        public async Task NewCustomer_NotFoundResult_ShouldReturnFalse()
        {
            //Arrange
            DefaultSetupRealHttpClient(HttpStatusCode.NotFound);
            var expectedUri = new Uri("http://test/api/Customer/");

            //Act
            var result = await facade.NewCustomer(customer);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Once(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post && req.RequestUri == expectedUri), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete && req.RequestUri == expectedUri), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI", 
                "CustomerOrderingScope"), Times.Once);
        }

        [Fact]
        public async Task NewCustomer_Null_ShouldReturnFalse()
        {
            //Arrange
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/api/Customer");

            //Act
            var result = await facade.NewCustomer(null);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task NewCustomer_UriNull_ShouldFalse()
        {
            //Arrange
            customerUriValue = null;
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/api/Customer/");
            
            //Act
            var result = await facade.NewCustomer(customer);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI",
                "CustomerOrderingScope"), Times.Never);
        }

        [Fact]
        public async Task NewCustomer_UriEmpty_ShouldFalse()
        {
            //Arrange
            customerUriValue = "";
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/api/Customer/");

            //Act
            var result = await facade.NewCustomer(customer);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI",
                "CustomerOrderingScope"), Times.Never);
        }

        [Fact]
        public async Task NewCustomer_AuthKeyNull_ShouldFalse()
        {
            //Arrange
            customerAuthServerUrlKeyValue = null;
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/api/Customer/");

            //Act
            var result = await facade.NewCustomer(customer);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI",
                "CustomerOrderingScope"), Times.Never);
        }

        [Fact]
        public async Task NewCustomer_AuthKeyEmpty_ShouldFalse()
        {
            //Arrange
            customerAuthServerUrlKeyValue = "";
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/api/Customer/");

            //Act
            var result = await facade.NewCustomer(customer);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI",
                "CustomerOrderingScope"), Times.Never);
        }

        [Fact]
        public async Task NewCustomer_ApiKeyNull_ShouldFalse()
        {
            //Arrange
            customerOrderingApiKeyValue = null;
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/api/Customer/");

            //Act
            var result = await facade.NewCustomer(customer);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI",
                "CustomerOrderingScope"), Times.Never);
        }

        [Fact]
        public async Task NewCustomer_ApiKeyEmpty_ShouldFalse()
        {
            //Arrange
            customerOrderingApiKeyValue = "";
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/api/Customer/");

            //Act
            var result = await facade.NewCustomer(customer);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI",
                "CustomerOrderingScope"), Times.Never);
        }

        [Fact]
        public async Task NewCustomer_OrderingScopeNull_ShouldFalse()
        {
            //Arrange
            customerOrderingScopeKeyValue = null;
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/api/Customer/");

            //Act
            var result = await facade.NewCustomer(customer);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI",
                "CustomerOrderingScope"), Times.Never);
        }

        [Fact]
        public async Task NewCustomer_OrderingScopeEmpty_ShouldFalse()
        {
            //Arrange
            customerOrderingScopeKeyValue = "";
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/api/Customer/");

            //Act
            var result = await facade.NewCustomer(customer);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI",
                "CustomerOrderingScope"), Times.Never);
        }

        [Fact]
        public async Task EditCustomer_Null_ShouldReturnFalse()
        {
            //Arrange
            DefaultSetupRealHttpClient(HttpStatusCode.NotFound);
            var expectedUri = new Uri("http://test/api/Customer");

            //Act
            var result = await facade.EditCustomer(null);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task EditCustomer_OKResult_ShouldReturnTrue()
        {
            //Arrange
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/api/Customer/1");

            //Act
            var result = await facade.EditCustomer(customer);

            //Assert
            Assert.True(true == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Once(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put && req.RequestUri == expectedUri), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI",
                "CustomerOrderingScope"), Times.Once);
        }

        [Fact]
        public async Task EditCustomer_NotFoundResult_ShouldFalse()
        {
            //Arrange
            DefaultSetupRealHttpClient(HttpStatusCode.NotFound);
            var expectedUri = new Uri("http://test/api/Customer/1");

            //Act
            var result = await facade.EditCustomer(customer);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Once(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI",
                "CustomerOrderingScope"), Times.Once);
        }

        [Fact]
        public async Task EditCustomer_UriNull_ShouldFalse()
        {
            //Arrange
            customerUriValue = null;
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/api/Customer/1");

            //Act
            var result = await facade.EditCustomer(customer);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI",
                "CustomerOrderingScope"), Times.Never);
        }

        [Fact]
        public async Task EditCustomer_UriEmpty_ShouldFalse()
        {
            //Arrange
            customerUriValue = "";
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/api/Customer/1");

            //Act
            var result = await facade.EditCustomer(customer);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI",
                "CustomerOrderingScope"), Times.Never);
        }

        [Fact]
        public async Task EditCustomer_AuthKeyNull_ShouldFalse()
        {
            //Arrange
            customerAuthServerUrlKeyValue = null;
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/api/Customer/1");

            //Act
            var result = await facade.EditCustomer(customer);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI",
                "CustomerOrderingScope"), Times.Never);
        }

        [Fact]
        public async Task EditCustomer_AuthKeyEmpty_ShouldFalse()
        {
            //Arrange
            customerAuthServerUrlKeyValue = "";
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/api/Customer/1");

            //Act
            var result = await facade.EditCustomer(customer);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI",
                "CustomerOrderingScope"), Times.Never);
        }

        [Fact]
        public async Task EditCustomer_ApiKeyNull_ShouldFalse()
        {
            //Arrange
            customerOrderingApiKeyValue = null;
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/api/Customer/1");

            //Act
            var result = await facade.EditCustomer(customer);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI",
                "CustomerOrderingScope"), Times.Never);
        }

        [Fact]
        public async Task EditCustomer_ApiKeyEmpty_ShouldFalse()
        {
            //Arrange
            customerOrderingApiKeyValue = "";
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/api/Customer/1");

            //Act
            var result = await facade.EditCustomer(customer);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI",
                "CustomerOrderingScope"), Times.Never);
        }

        [Fact]
        public async Task EditCustomer_OrderingScopeNull_ShouldFalse()
        {
            //Arrange
            customerOrderingScopeKeyValue = null;
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/api/Customer/1");

            //Act
            var result = await facade.EditCustomer(customer);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI",
                "CustomerOrderingScope"), Times.Never);
        }

        [Fact]
        public async Task EditCustomer_OrderingScopeEmpty_ShouldFalse()
        {
            //Arrange
            customerOrderingScopeKeyValue = "";
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/api/Customer/1");

            //Act
            var result = await facade.EditCustomer(customer);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI",
                "CustomerOrderingScope"), Times.Never);
        }

        [Fact]
        public async Task DeleteCustomer_OKResult_ShouldReturnTrue()
        {
            //Arrange
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/api/Customer/1");

            //Act
            var result = await facade.DeleteCustomer(customer.CustomerId);

            //Assert
            Assert.True(true == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Once(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete && req.RequestUri == expectedUri), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI",
                "CustomerOrderingScope"), Times.Once);
        }

        [Fact]
        public async Task DeleteCustomer_NotFoundResult_ShouldFalse()
        {
            //Arrange
            DefaultSetupRealHttpClient(HttpStatusCode.NotFound);
            var expectedUri = new Uri("http://test/api/Customer/1");

            //Act
            var result = await facade.DeleteCustomer(customer.CustomerId);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Once(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI",
                "CustomerOrderingScope"), Times.Once);
        }

        [Fact]
        public async Task DeleteCustomer_UriNull_ShouldFalse()
        {
            //Arrange
            customerUriValue = null;
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/api/Customer/1");

            //Act
            var result = await facade.DeleteCustomer(customer.CustomerId);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI",
                "CustomerOrderingScope"), Times.Never);
        }

        [Fact]
        public async Task DeleteCustomer_UriEmpty_ShouldFalse()
        {
            //Arrange
            customerUriValue = "";
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/api/Customer/1");

            //Act
            var result = await facade.DeleteCustomer(customer.CustomerId);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI",
                "CustomerOrderingScope"), Times.Never);
        }

        [Fact]
        public async Task DeleteCustomer_AuthKeyNull_ShouldFalse()
        {
            //Arrange
            customerAuthServerUrlKeyValue = null;
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/api/Customer/1");

            //Act
            var result = await facade.DeleteCustomer(customer.CustomerId);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI",
                "CustomerOrderingScope"), Times.Never);
        }

        [Fact]
        public async Task DeleteCustomer_AuthKeyEmpty_ShouldFalse()
        {
            //Arrange
            customerAuthServerUrlKeyValue = "";
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/api/Customer/1");

            //Act
            var result = await facade.DeleteCustomer(customer.CustomerId);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI",
                "CustomerOrderingScope"), Times.Never);
        }

        [Fact]
        public async Task DeleteCustomer_ApiKeyNull_ShouldFalse()
        {
            //Arrange
            customerOrderingApiKeyValue = null;
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/api/Customer/1");

            //Act
            var result = await facade.DeleteCustomer(customer.CustomerId);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI",
                "CustomerOrderingScope"), Times.Never);
        }

        [Fact]
        public async Task DeleteCustomer_ApiKeyEmpty_ShouldFalse()
        {
            //Arrange
            customerOrderingApiKeyValue = "";
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/api/Customer/1");

            //Act
            var result = await facade.DeleteCustomer(customer.CustomerId);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI",
                "CustomerOrderingScope"), Times.Never);
        }

        [Fact]
        public async Task DeleteCustomer_OrderingScopeNull_ShouldFalse()
        {
            //Arrange
            customerOrderingScopeKeyValue = null;
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/api/Customer/1");

            //Act
            var result = await facade.DeleteCustomer(customer.CustomerId);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI",
                "CustomerOrderingScope"), Times.Never);
        }

        [Fact]
        public async Task DeleteCustomer_OrderingScopeEmpty_ShouldFalse()
        {
            //Arrange
            customerOrderingScopeKeyValue = "";
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/api/Customer/1");

            //Act
            var result = await facade.DeleteCustomer(customer.CustomerId);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient("CustomerAuthServerUrl", "CustomerOrderingAPI",
                "CustomerOrderingScope"), Times.Never);
        }
    }
}