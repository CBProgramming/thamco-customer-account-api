using Customer.OrderFacade;
using Customer.OrderFacade.Models;
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
        public Mock<HttpMessageHandler> mockHandler;
        public IOrderFacade facade;
        private IConfiguration config;


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

        private void SetupHttpClient(HttpResponseMessage expected)
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
                {"Key1", "Value1"},
                {"Nested:Key1", "NestedValue1"},
                {"Nested:Key2", "NestedValue2"}
            };

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
            SetupHttpClient(expectedResult);
            SetupHttpFactoryMock(client);
            SetupConfig();
            facade = new OrderFacade.OrderFacade(mockFactory.Object, config);
        }

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
        public async Task NewCustomer_NotFoundResult_ShouldReturnTrue()
        {
            //Arrange
            DefaultSetup(HttpStatusCode.NotFound);
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
    }
}