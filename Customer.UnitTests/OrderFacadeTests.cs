using Customer.OrderFacade;
using Customer.OrderFacade.Models;
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
        private Mock<Task<DiscoveryDocumentResponse>> mockDisco;
        private Mock<Task<TokenResponse>> mockTokenResponse;
        private Task<TokenResponse> tokenResponse;
        private Mock<DiscoveryDocumentRequest> mockDiscoRequest;


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

        private void SetupConfig()
        {
            var myConfiguration = new Dictionary<string, string>
                        {
                            {"StaffAuthServerUrl", "https://fakeurl.com"},
                            {"ClientId", "ClientId"},
                            {"ClientSecret", "ClientSecret"}};

            config = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();
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
            //facade = new OrderFacade.OrderFacade(mockFactory.Object, config);
            SetupConfig();
        }

        /*        private void SetupTokenResponse()
                {
                    SetupRealHttpClient(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK
                    });
                    tokenResponse = client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
                    {
                        Address = "http://fakeendpoint.com",
                        ClientId = "clientId",
                        ClientSecret = "clientSecret",
                        Scope = "customer_ordering_api"
                    });
                }*/

        /*        private void SetupMockDiscoveryDocument()
                {
                    mockDisco = new Mock<Task<DiscoveryDocumentResponse>>(MockBehavior.Strict);
                }*/


        /* private void SetupMockHttpClient(HttpResponseMessage expected)
         {
             mockClient = new Mock<HttpClient>(MockBehavior.Strict);
             //mockClient.Setup(c => c.GetDiscoveryDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(mockDisco.Object).Verifiable();
             mockClient.Protected()
                 .Setup<Task<DiscoveryDocumentResponse>>(
                 "GetDiscoveryDocumentAsync",
                 //ItExpr.IsNull<HttpClient>(),
                 ItExpr.IsAny<string>(),
                 ItExpr.IsAny<CancellationToken>())
                 .Returns(mockDisco.Object)
                 .Verifiable();
             mockClient.Setup(c => c.RequestClientCredentialsTokenAsync(It.IsAny<ClientCredentialsTokenRequest>(), It.IsAny<CancellationToken>()))
                 .Returns(tokenResponse).Verifiable();
             mockClient.Setup(c => c.SetBearerToken(It.IsAny<string>())).Verifiable();
         }*/

        /*        private void DefaultSetupMockHttpClient(HttpStatusCode statusCode)
                {
                    SetupCustomer();
                    var expectedResult = new HttpResponseMessage
                    {
                        StatusCode = statusCode
                    };
                    SetMockMessageHandler(expectedResult);
                    SetupMockDiscoveryDocument();
                    SetupTokenResponse();
                    SetupMockHttpClient(expectedResult);
                    SetupHttpFactoryMock(client);
                    SetupConfig();
                    facade = new OrderFacade.OrderFacade(mockFactory.Object, config);
                }*/


        /*        [Fact]
                public async Task NewCustomer_OKResult_CheckAllMocks()
                {
                    //Arrange
                    DefaultSetupMockHttpClient(HttpStatusCode.OK);
                    var expectedUri = new Uri("http://test/api/Customer");

                    //Act
                    var result = await facade.NewCustomer(customer);

                    //Assert
                    Assert.True(true == result);
                    mockHandler.Protected().Verify("SendAsync",
                        Times.Once(),
                        ItExpr.Is<HttpRequestMessage>(
                            req => req.Method == HttpMethod.Post
                            && req.RequestUri == expectedUri),
                        ItExpr.IsAny<CancellationToken>());
                    mockFactory.Verify(factory => factory.CreateClient(It.IsAny<string>()), Times.Once);

        *//*            mockClient.Protected().Verify("GetDiscoveryDocumentAsync",
                        Times.Once(),
                        It.IsAny<string>(), 
                        It.IsAny<CancellationToken>());*//*
                    //mockClient.Verify(client => client.GetDiscoveryDocumentAsync(ItExpr.IsNull<HttpClient>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
                }*/

        [Fact]
        public async Task NewCustomer_OKResult_ShouldReturnTrue()
        {
            //Arrange
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/api/Customer");

            //Act
            var result = await facade.NewCustomer(customer);

            //Assert
            Assert.True(true == result);
            mockHandler.Protected().Verify("SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Post
                    && req.RequestUri == expectedUri),
                ItExpr.IsAny<CancellationToken>());
            mockFactory.Verify(factory => factory.CreateClient(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task NewCustomer_NotFoundResult_ShouldReturnFalse()
        {
            //Arrange
            DefaultSetupRealHttpClient(HttpStatusCode.NotFound);
            var expectedUri = new Uri("http://test/api/Customer");

            //Act
            var result = await facade.NewCustomer(customer);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Post
                    && req.RequestUri == expectedUri),
                ItExpr.IsAny<CancellationToken>());
            mockFactory.Verify(factory => factory.CreateClient(It.IsAny<string>()), Times.Once);
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
            mockHandler.Protected().Verify("SendAsync",
                Times.Never(),
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Post
                    && req.RequestUri == expectedUri),
                ItExpr.IsAny<CancellationToken>());
            mockFactory.Verify(factory => factory.CreateClient(It.IsAny<string>()), Times.Never);
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
            mockHandler.Protected().Verify("SendAsync",
                Times.Never(),
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Post
                    && req.RequestUri == expectedUri),
                ItExpr.IsAny<CancellationToken>());
            mockFactory.Verify(factory => factory.CreateClient(It.IsAny<string>()), Times.Never);
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
            mockHandler.Protected().Verify("SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Put
                    && req.RequestUri == expectedUri),
                ItExpr.IsAny<CancellationToken>());
            mockFactory.Verify(factory => factory.CreateClient(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task EditCustomer_NotFoundResult_ShouldReturnTrue()
        {
            //Arrange
            DefaultSetupRealHttpClient(HttpStatusCode.NotFound);
            var expectedUri = new Uri("http://test/api/Customer/1");

            //Act
            var result = await facade.EditCustomer(customer);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Put
                    && req.RequestUri == expectedUri),
                ItExpr.IsAny<CancellationToken>());
            mockFactory.Verify(factory => factory.CreateClient(It.IsAny<string>()), Times.Once);
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
            mockHandler.Protected().Verify("SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Delete
                    && req.RequestUri == expectedUri),
                ItExpr.IsAny<CancellationToken>());
            mockFactory.Verify(factory => factory.CreateClient(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task DeleteCustomer_NotFoundResult_ShouldReturnTrue()
        {
            //Arrange
            DefaultSetupRealHttpClient(HttpStatusCode.NotFound);
            var expectedUri = new Uri("http://test/api/Customer/1");

            //Act
            var result = await facade.DeleteCustomer(customer.CustomerId);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Delete
                    && req.RequestUri == expectedUri),
                ItExpr.IsAny<CancellationToken>());
            mockFactory.Verify(factory => factory.CreateClient(It.IsAny<string>()), Times.Once);
        }
    }
}