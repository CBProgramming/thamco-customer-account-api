using AspNet.Security.OpenIdConnect.Primitives;
using AutoMapper;
using Customer.AccountAPI;
using Customer.AccountAPI.Controllers;
using Customer.AccountAPI.Models;
using Customer.OrderFacade;
using Customer.Repository;
using Customer.Repository.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Customer.UnitTests
{
    public class CustomerControllerTests
    {

        private CustomerDto customerDto;
        private CustomerRepoModel customerRepoModel;
        private FakeCustomerRepository fakeRepo;
        private Mock<ICustomerRepository> mockRepo;
        private FakeOrderFacade fakeFacade;
        private Mock<IOrderFacade> mockFacade;
        private IMapper mapper;
        private ILogger<CustomerController> logger;
        private CustomerController controller;

        private void SetStandardCustomerDto()
        {
            customerDto = new CustomerDto
            {
                CustomerId = 1,
                CustomerAuthId = "fakeauthid",
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
                RequestedDeletion = false,
                CanPurchase = true,
                Active = true
            };
        }

        private void SetStandardCustomerRepoModel()
        {
            customerRepoModel = new CustomerRepoModel
            {
                CustomerId = 1,
                CustomerAuthId = "fakeauthid",
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
                RequestedDeletion = false,
                CanPurchase = true,
                Active = true
            };
        }

        private void SetFakeRepo(CustomerRepoModel customer)
        {
            fakeRepo = new FakeCustomerRepository
            {
                Customer = customer
            };
        }

        private void SetMapper()
        {
            mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            }).CreateMapper();
        }

        private void SetLogger()
        {
            logger = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider()
                .GetService<ILoggerFactory>()
                .CreateLogger<CustomerController>();
        }

        private void SetMockCustomerRepo (bool customerExists = true, bool customerActive = true, bool succeeds = true)
        {
            mockRepo = new Mock<ICustomerRepository>(MockBehavior.Strict);
            mockRepo.Setup(repo => repo.CustomerExists(It.IsAny<int>())).ReturnsAsync(customerExists).Verifiable();
            mockRepo.Setup(repo => repo.IsCustomerActive(It.IsAny<int>())).ReturnsAsync(customerActive).Verifiable();
            mockRepo.Setup(repo => repo.GetCustomer(It.IsAny<int>())).ReturnsAsync(customerRepoModel).Verifiable();
        }

        private void SetMockOrderFacade (bool customerExists = true, bool succeeds = true)
        {
            mockFacade = new Mock<IOrderFacade>(MockBehavior.Strict);
        }

        private void SetupUser (CustomerController controller)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "name"),
                                        new Claim(ClaimTypes.Name, "name"),
                                        new Claim(OpenIdConnectConstants.Claims.Subject, "fakeauthid" )
                                   }, "TestAuth"));
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
        }

        private void DefaultSetup(bool withMocks = false)
        {
            SetStandardCustomerDto();
            SetStandardCustomerRepoModel();
            SetFakeRepo(customerRepoModel);
            SetMapper();
            SetLogger();
            SetMockCustomerRepo();
            SetMockOrderFacade();
            if (!withMocks)
            {
                controller = new CustomerController(logger, fakeRepo, mapper, fakeFacade);
            }
            else
            {
                controller = new CustomerController(logger, mockRepo.Object, mapper, mockFacade.Object);
            }
            SetupUser(controller);
        }

        [Fact]
        public async void GetExistingCustomer_ShouldOk()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await controller.Get(1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var customer = objResult.Value as CustomerDto;
            Assert.NotNull(customer);
            Assert.Equal(customerRepoModel.CustomerId, customer.CustomerId);
            Assert.Equal(customerRepoModel.CustomerAuthId, customer.CustomerAuthId);
            Assert.Equal(customerRepoModel.GivenName, customer.GivenName);
            Assert.Equal(customerRepoModel.FamilyName, customer.FamilyName);
            Assert.Equal(customerRepoModel.AddressOne, customer.AddressOne);
            Assert.Equal(customerRepoModel.AddressTwo, customer.AddressTwo);
            Assert.Equal(customerRepoModel.Town, customer.Town);
            Assert.Equal(customerRepoModel.State, customer.State);
            Assert.Equal(customerRepoModel.AreaCode, customer.AreaCode);
            Assert.Equal(customerRepoModel.Country, customer.Country);
            Assert.Equal(customerRepoModel.EmailAddress, customer.EmailAddress);
            Assert.Equal(customerRepoModel.TelephoneNumber, customer.TelephoneNumber);
            Assert.Equal(customerRepoModel.RequestedDeletion, customer.RequestedDeletion);
            Assert.Equal(customerRepoModel.CanPurchase, customer.CanPurchase);
            Assert.Equal(customerRepoModel.Active, customer.Active);
        }

        [Fact]
        public async void GetExistingCustomer_VerifyMockCalls()
        {
            //Arrange
            DefaultSetup(true);
            int customerId = 1;

            //Act
            var result = await controller.Get(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var customer = objResult.Value as CustomerDto;
            Assert.NotNull(customer);
            Assert.Equal(customerRepoModel.CustomerId, customer.CustomerId);
            Assert.Equal(customerRepoModel.CustomerAuthId, customer.CustomerAuthId);
            Assert.Equal(customerRepoModel.GivenName, customer.GivenName);
            Assert.Equal(customerRepoModel.FamilyName, customer.FamilyName);
            Assert.Equal(customerRepoModel.AddressOne, customer.AddressOne);
            Assert.Equal(customerRepoModel.AddressTwo, customer.AddressTwo);
            Assert.Equal(customerRepoModel.Town, customer.Town);
            Assert.Equal(customerRepoModel.State, customer.State);
            Assert.Equal(customerRepoModel.AreaCode, customer.AreaCode);
            Assert.Equal(customerRepoModel.Country, customer.Country);
            Assert.Equal(customerRepoModel.EmailAddress, customer.EmailAddress);
            Assert.Equal(customerRepoModel.TelephoneNumber, customer.TelephoneNumber);
            Assert.Equal(customerRepoModel.RequestedDeletion, customer.RequestedDeletion);
            Assert.Equal(customerRepoModel.CanPurchase, customer.CanPurchase);
            Assert.Equal(customerRepoModel.Active, customer.Active);
            mockRepo.Verify();
            mockRepo.Verify(repo => repo.CustomerExists(customerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerId), Times.Once);
            mockRepo.Verify(repo => repo.GetCustomer(customerId), Times.Once);
        }

        [Fact]
        public async void GetCustomer_DoesntExist_ShouldNotFound()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await controller.Get(2);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void GetCustomer_DoesntExist_VerifyMocks()
        {
            //Arrange
            DefaultSetup(true);
            SetMockCustomerRepo(customerExists: false, customerActive: true, succeeds: true);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockFacade.Object);
            int customerId = 2;

            //Act
            var result = await controller.Get(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerId), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomer(customerId), Times.Never);
        }

        [Fact]
        public async void GetCustomer_NotActive_ShouldNotFound()
        {
            //Arrange
            DefaultSetup();
            customerRepoModel.Active = false;

            //Act
            var result = await controller.Get(1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void GetCustomer_NotActive_VerifyMocks()
        {
            //Arrange
            DefaultSetup();
            SetMockCustomerRepo(customerExists: true, customerActive: false, succeeds: true);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockFacade.Object);
            int customerId = 1;

            //Act
            var result = await controller.Get(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerId), Times.Once);
            mockRepo.Verify(repo => repo.GetCustomer(customerId), Times.Never);
        }

        [Fact]
        public async void GetCustomer_InvaildTokenId_ShouldForbid()
        {
            //Arrange
            DefaultSetup();
            customerRepoModel.CustomerAuthId = "officialId";

            //Act
            var result = await controller.Get(1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void GetCustomer_InvaildTokenId_VerifyMocks()
        {
            //Arrange
            DefaultSetup();
            SetMockCustomerRepo(customerExists: true, customerActive: true, succeeds: true);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockFacade.Object);
            SetupUser(controller);
            int customerId = 1;
            customerRepoModel.CustomerAuthId = "officialId";

            //Act
            var result = await controller.Get(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerId), Times.Once);
            mockRepo.Verify(repo => repo.GetCustomer(customerId), Times.Once);
        }
    }
}
