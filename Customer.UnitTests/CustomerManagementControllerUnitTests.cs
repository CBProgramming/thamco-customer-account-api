using AspNet.Security.OpenIdConnect.Primitives;
using AutoMapper;
using Customer.AccountAPI;
using Customer.AccountAPI.Controllers;
using Customer.AccountAPI.Models;
using Customer.AuthFacade;
using Customer.OrderFacade;
using Customer.OrderFacade.Models;
using Customer.Repository;
using Customer.Repository.Models;
using Customer.ReviewFacade;
using Customer.ReviewFacade.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace Customer.UnitTests
{
    public class CustomerManagementControllerUnitTests
    {
        private CustomerDto customerDto;
        private IConfiguration config;
        private CustomerRepoModel customerRepoModel;
        private FakeReviewCustomerFacade fakeReviewFacade;
        private Mock<IReviewCustomerFacade> mockReviewFacade;
        private FakeCustomerRepository fakeRepo;
        private Mock<ICustomerRepository> mockRepo;
        private FakeAuthFacade fakeAuthFacade;
        private Mock<IAuthFacade> mockAuthFacade;
        private FakeOrderFacade fakeOrderFacade;
        private Mock<IOrderFacade> mockOrderFacade;
        private IMapper mapper;
        private ILogger<CustomerManagementController> logger;
        private CustomerManagementController controller;

        private void SetupConfig()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                {"AnonymisedString", "Anonymised"},
                {"AnonymisedPhone", "00000000000"},
                {"AnonymisedEmail", "anon@anon.com"},
            };
            config = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }

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

        private CustomerDto GetEditedDetailsDto()
        {
            return new CustomerDto
            {
                CustomerId = 1,
                CustomerAuthId = "fakeauthid",
                GivenName = "NewName",
                FamilyName = "NewName",
                AddressOne = "NewAddress",
                AddressTwo = "NewAddress",
                Town = "NewTown",
                State = "NewState",
                AreaCode = "New Area Code",
                Country = "New Country",
                EmailAddress = "newemail@email.com",
                TelephoneNumber = "07000000000",
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
                .CreateLogger<CustomerManagementController>();
        }

        private void SetMockCustomerRepo(bool customerExists = true, bool customerActive = true, bool succeeds = true, bool authMatch = true)
        {
            mockRepo = new Mock<ICustomerRepository>(MockBehavior.Strict);
            mockRepo.Setup(repo => repo.CustomerExists(It.IsAny<int>())).ReturnsAsync(customerExists).Verifiable();
            mockRepo.Setup(repo => repo.IsCustomerActive(It.IsAny<int>())).ReturnsAsync(customerActive).Verifiable();
            mockRepo.Setup(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>())).ReturnsAsync(succeeds).Verifiable();
            mockRepo.Setup(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>())).ReturnsAsync(succeeds).Verifiable();
            mockRepo.Setup(repo => repo.MatchingAuthId(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(authMatch).Verifiable();
            mockRepo.Setup(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>())).ReturnsAsync(succeeds).Verifiable();
            if (succeeds && customerExists)
            {
                mockRepo.Setup(repo => repo.GetCustomer(It.IsAny<int>())).ReturnsAsync(customerRepoModel).Verifiable();
            }
            else
            {
                mockRepo.Setup(repo => repo.GetCustomer(It.IsAny<int>())).ReturnsAsync((CustomerRepoModel)null).Verifiable();
            }
        }

        private void SetMockReviewFacade(bool customerExists = true, bool succeeds = true)
        {
            mockReviewFacade = new Mock<IReviewCustomerFacade>(MockBehavior.Strict);
            mockReviewFacade.Setup(facade => facade.NewCustomer(It.IsAny<ReviewCustomerDto>())).ReturnsAsync(succeeds).Verifiable();
            mockReviewFacade.Setup(facade => facade.EditCustomer(It.IsAny<ReviewCustomerDto>())).ReturnsAsync(succeeds).Verifiable();
            mockReviewFacade.Setup(facade => facade.DeleteCustomer(It.IsAny<int>())).ReturnsAsync(succeeds).Verifiable();
        }

        private void SetMockOrderFacade(bool customerExists = true, bool succeeds = true)
        {
            mockOrderFacade = new Mock<IOrderFacade>(MockBehavior.Strict);
            mockOrderFacade.Setup(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>())).ReturnsAsync(succeeds).Verifiable();
            mockOrderFacade.Setup(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>())).ReturnsAsync(succeeds).Verifiable();
            mockOrderFacade.Setup(facade => facade.DeleteCustomer(It.IsAny<int>())).ReturnsAsync(succeeds).Verifiable();
        }

        private void SetMockAuthFacade(bool customerExists = true, bool succeeds = true)
        {
            mockAuthFacade = new Mock<IAuthFacade>(MockBehavior.Strict);
            mockAuthFacade.Setup(facade => facade.DeleteAccount(It.IsAny<string>())).ReturnsAsync(succeeds).Verifiable();
        }

        private void SetupUser(CustomerManagementController controller)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "name"),
                                        new Claim(ClaimTypes.Name, "name"),
                                        new Claim(OpenIdConnectConstants.Claims.Subject, "fakeauthid" )
                                   }, "TestAuth"));
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
        }

        private void SetFakeFacade()
        {
            fakeOrderFacade = new FakeOrderFacade();
        }

        private void SetFakeReviewFacade()
        {
            fakeReviewFacade = new FakeReviewCustomerFacade();
        }

        private void SetFakeAuthFacade()
        {
            fakeAuthFacade = new FakeAuthFacade();
        }

        private void DefaultSetup(bool withMocks = false)
        {
            SetupConfig();
            SetStandardCustomerDto();
            SetStandardCustomerRepoModel();
            SetFakeRepo(customerRepoModel);
            SetFakeFacade();
            SetFakeReviewFacade();
            SetFakeAuthFacade();
            SetMapper();
            SetLogger();
            SetMockCustomerRepo();
            SetMockOrderFacade();
            SetMockReviewFacade();
            SetMockAuthFacade();
            if (!withMocks)
            {
                controller = new CustomerManagementController(config, logger, fakeRepo, mapper, 
                    fakeOrderFacade, fakeReviewFacade, fakeAuthFacade);
            }
            else
            {
                controller = new CustomerManagementController(config, logger, mockRepo.Object, 
                    mapper, mockOrderFacade.Object, mockReviewFacade.Object,
                    mockAuthFacade.Object);
            }
            SetupUser(controller);
        }

        [Fact]
        public async void BlockPurchases_CustomerDoesntExist_NotFound()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await controller.Put(2, false);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Customer.CustomerId, customerRepoModel.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, customerRepoModel.CustomerAuthId);
            Assert.Equal(fakeRepo.Customer.GivenName, customerRepoModel.GivenName);
            Assert.Equal(fakeRepo.Customer.FamilyName, customerRepoModel.FamilyName);
            Assert.Equal(fakeRepo.Customer.AddressOne, customerRepoModel.AddressOne);
            Assert.Equal(fakeRepo.Customer.AddressTwo, customerRepoModel.AddressTwo);
            Assert.Equal(fakeRepo.Customer.Town, customerRepoModel.Town);
            Assert.Equal(fakeRepo.Customer.State, customerRepoModel.State);
            Assert.Equal(fakeRepo.Customer.AreaCode, customerRepoModel.AreaCode);
            Assert.Equal(fakeRepo.Customer.Country, customerRepoModel.Country);
            Assert.Equal(fakeRepo.Customer.EmailAddress, customerRepoModel.EmailAddress);
            Assert.Equal(fakeRepo.Customer.TelephoneNumber, customerRepoModel.TelephoneNumber);
            Assert.Equal(fakeRepo.Customer.RequestedDeletion, customerRepoModel.RequestedDeletion);
            Assert.Equal(fakeRepo.Customer.CanPurchase, customerRepoModel.CanPurchase);
            Assert.Equal(fakeRepo.Customer.Active, customerRepoModel.Active);
            Assert.Null(fakeAuthFacade.CustomerAuthId);
            Assert.Equal(0, fakeOrderFacade.CustomerId);
            Assert.Null(fakeOrderFacade.Customer);
            Assert.Equal(0, fakeReviewFacade.CustomerId);
            Assert.Null(fakeReviewFacade.Customer);
        }

        [Fact]
        public async void BlockPurchases_CustomerDoesntExist_VerifyMocks()
        {
            //Arrange
            DefaultSetup(true);
            SetMockCustomerRepo(customerExists: false, customerActive: false);
            controller = new CustomerManagementController(config, logger, mockRepo.Object, mapper, 
                mockOrderFacade.Object, mockReviewFacade.Object, mockAuthFacade.Object);
            SetupUser(controller);

            //Act
            var result = await controller.Put(customerDto.CustomerId, false);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.MatchingAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Once);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewCustomer(It.IsAny<ReviewCustomerDto>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.EditCustomer(It.IsAny<ReviewCustomerDto>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
            mockAuthFacade.Verify(facade => facade.DeleteAccount(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async void BlockPurchases__ShouldOk()
        {
            //Arrange
            DefaultSetup();
            bool canPurchase = false;

            //Act
            var result = await controller.Put(customerDto.CustomerId, canPurchase);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Customer);
            Assert.Equal(fakeRepo.Customer.CustomerId, customerRepoModel.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, customerRepoModel.CustomerAuthId);
            Assert.Equal(fakeRepo.Customer.GivenName, customerRepoModel.GivenName);
            Assert.Equal(fakeRepo.Customer.FamilyName, customerRepoModel.FamilyName);
            Assert.Equal(fakeRepo.Customer.AddressOne, customerRepoModel.AddressOne);
            Assert.Equal(fakeRepo.Customer.AddressTwo, customerRepoModel.AddressTwo);
            Assert.Equal(fakeRepo.Customer.Town, customerRepoModel.Town);
            Assert.Equal(fakeRepo.Customer.State, customerRepoModel.State);
            Assert.Equal(fakeRepo.Customer.AreaCode, customerRepoModel.AreaCode);
            Assert.Equal(fakeRepo.Customer.Country, customerRepoModel.Country);
            Assert.Equal(fakeRepo.Customer.EmailAddress, customerRepoModel.EmailAddress);
            Assert.Equal(fakeRepo.Customer.TelephoneNumber, customerRepoModel.TelephoneNumber);
            Assert.Equal(fakeRepo.Customer.RequestedDeletion, customerRepoModel.RequestedDeletion);
            Assert.NotEqual(fakeRepo.Customer.CanPurchase, customerRepoModel.CanPurchase);
            Assert.Equal(fakeRepo.Customer.Active, customerRepoModel.Active);
            Assert.Null(fakeAuthFacade.CustomerAuthId);
            Assert.Equal(0, fakeOrderFacade.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerId, fakeOrderFacade.Customer.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, fakeOrderFacade.Customer.CustomerAuthId);
            Assert.Equal(fakeRepo.Customer.GivenName, fakeOrderFacade.Customer.GivenName);
            Assert.Equal(fakeRepo.Customer.FamilyName, fakeOrderFacade.Customer.FamilyName);
            Assert.Equal(fakeRepo.Customer.AddressOne, fakeOrderFacade.Customer.AddressOne);
            Assert.Equal(fakeRepo.Customer.AddressTwo, fakeOrderFacade.Customer.AddressTwo);
            Assert.Equal(fakeRepo.Customer.Town, fakeOrderFacade.Customer.Town);
            Assert.Equal(fakeRepo.Customer.State, fakeOrderFacade.Customer.State);
            Assert.Equal(fakeRepo.Customer.AreaCode, fakeOrderFacade.Customer.AreaCode);
            Assert.Equal(fakeRepo.Customer.Country, fakeOrderFacade.Customer.Country);
            Assert.Equal(fakeRepo.Customer.EmailAddress, fakeOrderFacade.Customer.EmailAddress);
            Assert.Equal(fakeRepo.Customer.TelephoneNumber, fakeOrderFacade.Customer.TelephoneNumber);
            Assert.Equal(canPurchase, fakeOrderFacade.Customer.CanPurchase);
            Assert.Equal(fakeRepo.Customer.Active, fakeOrderFacade.Customer.Active);
            Assert.NotNull(fakeOrderFacade.Customer);
            Assert.Null(fakeReviewFacade.Customer);
            Assert.Equal(0, fakeReviewFacade.CustomerId);
        }

        [Fact]
        public async void BlockPurchases_VerifyMocks()
        {
            //Arrange
            DefaultSetup(true);
            SetMockCustomerRepo();
            controller = new CustomerManagementController(config, logger, mockRepo.Object, mapper,
                mockOrderFacade.Object, mockReviewFacade.Object, mockAuthFacade.Object);
            SetupUser(controller);

            //Act
            var result = await controller.Put(customerDto.CustomerId, false);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.MatchingAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Once);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Once);
            mockOrderFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewCustomer(It.IsAny<ReviewCustomerDto>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.EditCustomer(It.IsAny<ReviewCustomerDto>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
            mockAuthFacade.Verify(facade => facade.DeleteAccount(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async void BlockPurchases_CustomerIsInactive_ShouldOk()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer.Active = false;
            bool canPurchase = false;

            //Act
            var result = await controller.Put(customerDto.CustomerId, canPurchase);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Customer);
            Assert.Equal(fakeRepo.Customer.CustomerId, customerRepoModel.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, customerRepoModel.CustomerAuthId);
            Assert.Equal(fakeRepo.Customer.GivenName, customerRepoModel.GivenName);
            Assert.Equal(fakeRepo.Customer.FamilyName, customerRepoModel.FamilyName);
            Assert.Equal(fakeRepo.Customer.AddressOne, customerRepoModel.AddressOne);
            Assert.Equal(fakeRepo.Customer.AddressTwo, customerRepoModel.AddressTwo);
            Assert.Equal(fakeRepo.Customer.Town, customerRepoModel.Town);
            Assert.Equal(fakeRepo.Customer.State, customerRepoModel.State);
            Assert.Equal(fakeRepo.Customer.AreaCode, customerRepoModel.AreaCode);
            Assert.Equal(fakeRepo.Customer.Country, customerRepoModel.Country);
            Assert.Equal(fakeRepo.Customer.EmailAddress, customerRepoModel.EmailAddress);
            Assert.Equal(fakeRepo.Customer.TelephoneNumber, customerRepoModel.TelephoneNumber);
            Assert.Equal(fakeRepo.Customer.RequestedDeletion, customerRepoModel.RequestedDeletion);
            Assert.NotEqual(fakeRepo.Customer.CanPurchase, customerRepoModel.CanPurchase);
            Assert.Equal(fakeRepo.Customer.Active, customerRepoModel.Active);
            Assert.Null(fakeAuthFacade.CustomerAuthId);
            Assert.Equal(0, fakeOrderFacade.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerId, fakeOrderFacade.Customer.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, fakeOrderFacade.Customer.CustomerAuthId);
            Assert.Equal(fakeRepo.Customer.GivenName, fakeOrderFacade.Customer.GivenName);
            Assert.Equal(fakeRepo.Customer.FamilyName, fakeOrderFacade.Customer.FamilyName);
            Assert.Equal(fakeRepo.Customer.AddressOne, fakeOrderFacade.Customer.AddressOne);
            Assert.Equal(fakeRepo.Customer.AddressTwo, fakeOrderFacade.Customer.AddressTwo);
            Assert.Equal(fakeRepo.Customer.Town, fakeOrderFacade.Customer.Town);
            Assert.Equal(fakeRepo.Customer.State, fakeOrderFacade.Customer.State);
            Assert.Equal(fakeRepo.Customer.AreaCode, fakeOrderFacade.Customer.AreaCode);
            Assert.Equal(fakeRepo.Customer.Country, fakeOrderFacade.Customer.Country);
            Assert.Equal(fakeRepo.Customer.EmailAddress, fakeOrderFacade.Customer.EmailAddress);
            Assert.Equal(fakeRepo.Customer.TelephoneNumber, fakeOrderFacade.Customer.TelephoneNumber);
            Assert.Equal(canPurchase, fakeOrderFacade.Customer.CanPurchase);
            Assert.Equal(fakeRepo.Customer.Active, fakeOrderFacade.Customer.Active);
            Assert.NotNull(fakeOrderFacade.Customer);
            Assert.Null(fakeReviewFacade.Customer);
        }

        [Fact]
        public async void BlockPurchases_CustomerIsInactive_VerifyMocks()
        {
            //Arrange
            DefaultSetup(true);
            SetMockCustomerRepo(customerActive: false);
            controller = new CustomerManagementController(config, logger, mockRepo.Object, mapper,
                mockOrderFacade.Object, mockReviewFacade.Object, mockAuthFacade.Object);
            SetupUser(controller);

            //Act
            var result = await controller.Put(customerDto.CustomerId, false);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.MatchingAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Once);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Once);
            mockOrderFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewCustomer(It.IsAny<ReviewCustomerDto>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.EditCustomer(It.IsAny<ReviewCustomerDto>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
            mockAuthFacade.Verify(facade => facade.DeleteAccount(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async void AllowPurchases_CustomerDoesntExist_NotFound()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer.CanPurchase = false;
            customerRepoModel.CanPurchase = false;

            //Act
            var result = await controller.Put(2, true);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Customer.CustomerId, customerRepoModel.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, customerRepoModel.CustomerAuthId);
            Assert.Equal(fakeRepo.Customer.GivenName, customerRepoModel.GivenName);
            Assert.Equal(fakeRepo.Customer.FamilyName, customerRepoModel.FamilyName);
            Assert.Equal(fakeRepo.Customer.AddressOne, customerRepoModel.AddressOne);
            Assert.Equal(fakeRepo.Customer.AddressTwo, customerRepoModel.AddressTwo);
            Assert.Equal(fakeRepo.Customer.Town, customerRepoModel.Town);
            Assert.Equal(fakeRepo.Customer.State, customerRepoModel.State);
            Assert.Equal(fakeRepo.Customer.AreaCode, customerRepoModel.AreaCode);
            Assert.Equal(fakeRepo.Customer.Country, customerRepoModel.Country);
            Assert.Equal(fakeRepo.Customer.EmailAddress, customerRepoModel.EmailAddress);
            Assert.Equal(fakeRepo.Customer.TelephoneNumber, customerRepoModel.TelephoneNumber);
            Assert.Equal(fakeRepo.Customer.RequestedDeletion, customerRepoModel.RequestedDeletion);
            Assert.Equal(fakeRepo.Customer.CanPurchase, customerRepoModel.CanPurchase);
            Assert.Equal(fakeRepo.Customer.Active, customerRepoModel.Active);
            Assert.Null(fakeAuthFacade.CustomerAuthId);
            Assert.Equal(0, fakeOrderFacade.CustomerId);
            Assert.Null(fakeOrderFacade.Customer);
            Assert.Equal(0, fakeReviewFacade.CustomerId);
            Assert.Null(fakeReviewFacade.Customer);
        }

        [Fact]
        public async void AllowPurchases_CustomerDoesntExist_VerifyMocks()
        {
            //Arrange
            DefaultSetup(true);
            SetMockCustomerRepo(customerExists: false, customerActive: false);
            controller = new CustomerManagementController(config, logger, mockRepo.Object, mapper,
                mockOrderFacade.Object, mockReviewFacade.Object, mockAuthFacade.Object);
            SetupUser(controller);

            //Act
            var result = await controller.Put(customerDto.CustomerId, true);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.MatchingAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Once);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewCustomer(It.IsAny<ReviewCustomerDto>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.EditCustomer(It.IsAny<ReviewCustomerDto>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
            mockAuthFacade.Verify(facade => facade.DeleteAccount(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async void AllowPurchases__ShouldOk()
        {
            //Arrange
            DefaultSetup();
            bool canPurchase = false;
            fakeRepo.Customer.CanPurchase = false;
            customerRepoModel.CanPurchase = false;

            //Act
            var result = await controller.Put(customerDto.CustomerId, canPurchase);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Customer);
            Assert.Equal(fakeRepo.Customer.CustomerId, customerRepoModel.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, customerRepoModel.CustomerAuthId);
            Assert.Equal(fakeRepo.Customer.GivenName, customerRepoModel.GivenName);
            Assert.Equal(fakeRepo.Customer.FamilyName, customerRepoModel.FamilyName);
            Assert.Equal(fakeRepo.Customer.AddressOne, customerRepoModel.AddressOne);
            Assert.Equal(fakeRepo.Customer.AddressTwo, customerRepoModel.AddressTwo);
            Assert.Equal(fakeRepo.Customer.Town, customerRepoModel.Town);
            Assert.Equal(fakeRepo.Customer.State, customerRepoModel.State);
            Assert.Equal(fakeRepo.Customer.AreaCode, customerRepoModel.AreaCode);
            Assert.Equal(fakeRepo.Customer.Country, customerRepoModel.Country);
            Assert.Equal(fakeRepo.Customer.EmailAddress, customerRepoModel.EmailAddress);
            Assert.Equal(fakeRepo.Customer.TelephoneNumber, customerRepoModel.TelephoneNumber);
            Assert.Equal(fakeRepo.Customer.RequestedDeletion, customerRepoModel.RequestedDeletion);
            Assert.Equal(fakeRepo.Customer.CanPurchase, customerRepoModel.CanPurchase);
            Assert.Equal(fakeRepo.Customer.Active, customerRepoModel.Active);
            Assert.Null(fakeAuthFacade.CustomerAuthId);
            Assert.Equal(0, fakeOrderFacade.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerId, fakeOrderFacade.Customer.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, fakeOrderFacade.Customer.CustomerAuthId);
            Assert.Equal(fakeRepo.Customer.GivenName, fakeOrderFacade.Customer.GivenName);
            Assert.Equal(fakeRepo.Customer.FamilyName, fakeOrderFacade.Customer.FamilyName);
            Assert.Equal(fakeRepo.Customer.AddressOne, fakeOrderFacade.Customer.AddressOne);
            Assert.Equal(fakeRepo.Customer.AddressTwo, fakeOrderFacade.Customer.AddressTwo);
            Assert.Equal(fakeRepo.Customer.Town, fakeOrderFacade.Customer.Town);
            Assert.Equal(fakeRepo.Customer.State, fakeOrderFacade.Customer.State);
            Assert.Equal(fakeRepo.Customer.AreaCode, fakeOrderFacade.Customer.AreaCode);
            Assert.Equal(fakeRepo.Customer.Country, fakeOrderFacade.Customer.Country);
            Assert.Equal(fakeRepo.Customer.EmailAddress, fakeOrderFacade.Customer.EmailAddress);
            Assert.Equal(fakeRepo.Customer.TelephoneNumber, fakeOrderFacade.Customer.TelephoneNumber);
            Assert.Equal(canPurchase, fakeOrderFacade.Customer.CanPurchase);
            Assert.Equal(fakeRepo.Customer.Active, fakeOrderFacade.Customer.Active);
            Assert.NotNull(fakeOrderFacade.Customer);
            Assert.Null(fakeReviewFacade.Customer);
            Assert.Equal(0, fakeReviewFacade.CustomerId);
            
        }

        [Fact]
        public async void AllowPurchases_VerifyMocks()
        {
            //Arrange
            DefaultSetup(true);
            SetMockCustomerRepo();
            controller = new CustomerManagementController(config, logger, mockRepo.Object, mapper,
                mockOrderFacade.Object, mockReviewFacade.Object, mockAuthFacade.Object);
            SetupUser(controller);

            //Act
            var result = await controller.Put(customerDto.CustomerId, true);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.MatchingAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Once);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Once);
            mockOrderFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewCustomer(It.IsAny<ReviewCustomerDto>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.EditCustomer(It.IsAny<ReviewCustomerDto>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
            mockAuthFacade.Verify(facade => facade.DeleteAccount(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async void AllowPurchases_CustomerIsInactive_ShouldNotFound()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer.Active = false;
            fakeRepo.Customer.CanPurchase = false;
            customerRepoModel.CanPurchase = false;

            //Act
            var result = await controller.Put(customerDto.CustomerId, true);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Customer);
            Assert.Equal(fakeRepo.Customer.CustomerId, customerRepoModel.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, customerRepoModel.CustomerAuthId);
            Assert.Equal(fakeRepo.Customer.GivenName, customerRepoModel.GivenName);
            Assert.Equal(fakeRepo.Customer.FamilyName, customerRepoModel.FamilyName);
            Assert.Equal(fakeRepo.Customer.AddressOne, customerRepoModel.AddressOne);
            Assert.Equal(fakeRepo.Customer.AddressTwo, customerRepoModel.AddressTwo);
            Assert.Equal(fakeRepo.Customer.Town, customerRepoModel.Town);
            Assert.Equal(fakeRepo.Customer.State, customerRepoModel.State);
            Assert.Equal(fakeRepo.Customer.AreaCode, customerRepoModel.AreaCode);
            Assert.Equal(fakeRepo.Customer.Country, customerRepoModel.Country);
            Assert.Equal(fakeRepo.Customer.EmailAddress, customerRepoModel.EmailAddress);
            Assert.Equal(fakeRepo.Customer.TelephoneNumber, customerRepoModel.TelephoneNumber);
            Assert.Equal(fakeRepo.Customer.RequestedDeletion, customerRepoModel.RequestedDeletion);
            Assert.Equal(fakeRepo.Customer.CanPurchase, customerRepoModel.CanPurchase);
            Assert.Equal(fakeRepo.Customer.Active, customerRepoModel.Active);
            Assert.Null(fakeAuthFacade.CustomerAuthId);
            Assert.Equal(0, fakeOrderFacade.CustomerId);
            Assert.Null(fakeOrderFacade.Customer);
            Assert.Equal(0, fakeReviewFacade.CustomerId);
            Assert.Null(fakeReviewFacade.Customer);
        }

        [Fact]
        public async void AllowPurchases_CustomerIsInactive_VerifyMocks()
        {
            //Arrange
            DefaultSetup(true);
            customerRepoModel.Active = false;
            SetMockCustomerRepo(customerActive: false);
            controller = new CustomerManagementController(config, logger, mockRepo.Object, mapper,
                mockOrderFacade.Object, mockReviewFacade.Object, mockAuthFacade.Object);
            SetupUser(controller);

            //Act
            var result = await controller.Put(customerDto.CustomerId, true);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.MatchingAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Once);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewCustomer(It.IsAny<ReviewCustomerDto>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.EditCustomer(It.IsAny<ReviewCustomerDto>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
            mockAuthFacade.Verify(facade => facade.DeleteAccount(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async void DeleteExistingCustomer_ShouldOk()
        {
            //Arrange
            DefaultSetup();
            int customerId = 1;

            //Act
            var result = await controller.Delete(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Equal("Anonymised", fakeRepo.Customer.GivenName);
            Assert.Equal("Anonymised", fakeRepo.Customer.FamilyName);
            Assert.Equal("Anonymised", fakeRepo.Customer.AddressOne);
            Assert.Equal("Anonymised", fakeRepo.Customer.AddressTwo);
            Assert.Equal("Anonymised", fakeRepo.Customer.Town);
            Assert.Equal("Anonymised", fakeRepo.Customer.State);
            Assert.Equal("Anonymised", fakeRepo.Customer.AreaCode);
            Assert.Equal("Anonymised", fakeRepo.Customer.Country);
            Assert.Equal("anon@anon.com", fakeRepo.Customer.EmailAddress);
            Assert.Equal("00000000000", fakeRepo.Customer.TelephoneNumber);
            Assert.True(false == fakeRepo.Customer.RequestedDeletion);
            Assert.True(false == fakeRepo.Customer.CanPurchase);
            Assert.True(false == fakeRepo.Customer.Active);
            Assert.Equal("fakeauthid", fakeAuthFacade.CustomerAuthId);
            Assert.Equal(customerId, fakeOrderFacade.CustomerId);
            Assert.Null(fakeOrderFacade.Customer);
            Assert.Equal(customerId, fakeReviewFacade.CustomerId);
            Assert.Null(fakeReviewFacade.Customer);
        }

        [Fact]
        public async void DeleteExistingCustomer_VerifyMocks()
        {
            //Arrange
            DefaultSetup(true);
            int customerId = 1;

            //Act
            var result = await controller.Delete(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.MatchingAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Once);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Once);
            mockReviewFacade.Verify(facade => facade.NewCustomer(It.IsAny<ReviewCustomerDto>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.EditCustomer(It.IsAny<ReviewCustomerDto>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Once);
            mockAuthFacade.Verify(facade => facade.DeleteAccount(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async void DeleteCustomer_CustomerDoesntExist_ShouldNotFound()
        {
            //Arrange
            DefaultSetup();
            int customerId = 2;

            //Act
            var result = await controller.Delete(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            Assert.Equal(customerRepoModel.GivenName, fakeRepo.Customer.GivenName);
            Assert.Equal(customerRepoModel.FamilyName, fakeRepo.Customer.FamilyName);
            Assert.Equal(customerRepoModel.AddressOne, fakeRepo.Customer.AddressOne);
            Assert.Equal(customerRepoModel.AddressTwo, fakeRepo.Customer.AddressTwo);
            Assert.Equal(customerRepoModel.Town, fakeRepo.Customer.Town);
            Assert.Equal(customerRepoModel.State, fakeRepo.Customer.State);
            Assert.Equal(customerRepoModel.AreaCode, fakeRepo.Customer.AreaCode);
            Assert.Equal(customerRepoModel.Country, fakeRepo.Customer.Country);
            Assert.Equal(customerRepoModel.EmailAddress, fakeRepo.Customer.EmailAddress);
            Assert.Equal(customerRepoModel.TelephoneNumber, fakeRepo.Customer.TelephoneNumber);
            Assert.True(customerRepoModel.RequestedDeletion == fakeRepo.Customer.RequestedDeletion);
            Assert.True(customerRepoModel.CanPurchase == fakeRepo.Customer.CanPurchase);
            Assert.True(customerRepoModel.Active == fakeRepo.Customer.Active);
            Assert.Null(fakeAuthFacade.CustomerAuthId);
            Assert.Equal(0, fakeOrderFacade.CustomerId);
            Assert.Null(fakeOrderFacade.Customer);
            Assert.Equal(0, fakeReviewFacade.CustomerId);
            Assert.Null(fakeReviewFacade.Customer);
        }

        [Fact]
        public async void DeleteExistingCustomer_CustomerDoesntExist_VerifyMocks()
        {
            //Arrange
            DefaultSetup(true);
            SetMockCustomerRepo(customerExists: false);
            controller = new CustomerManagementController(config, logger, mockRepo.Object, mapper,
                mockOrderFacade.Object, mockReviewFacade.Object, mockAuthFacade.Object);
            SetupUser(controller);
            int customerId = 2;

            //Act
            var result = await controller.Delete(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.MatchingAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Once);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewCustomer(It.IsAny<ReviewCustomerDto>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.EditCustomer(It.IsAny<ReviewCustomerDto>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
            mockAuthFacade.Verify(facade => facade.DeleteAccount(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async void DeleteExistingCustomer_CustomerNotActive_ShouldOk()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer.Active = false;
            int customerId = 1;

            //Act
            var result = await controller.Delete(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Equal("Anonymised", fakeRepo.Customer.GivenName);
            Assert.Equal("Anonymised", fakeRepo.Customer.FamilyName);
            Assert.Equal("Anonymised", fakeRepo.Customer.AddressOne);
            Assert.Equal("Anonymised", fakeRepo.Customer.AddressTwo);
            Assert.Equal("Anonymised", fakeRepo.Customer.Town);
            Assert.Equal("Anonymised", fakeRepo.Customer.State);
            Assert.Equal("Anonymised", fakeRepo.Customer.AreaCode);
            Assert.Equal("Anonymised", fakeRepo.Customer.Country);
            Assert.Equal("anon@anon.com", fakeRepo.Customer.EmailAddress);
            Assert.Equal("00000000000", fakeRepo.Customer.TelephoneNumber);
            Assert.True(false == fakeRepo.Customer.RequestedDeletion);
            Assert.True(false == fakeRepo.Customer.CanPurchase);
            Assert.True(false == fakeRepo.Customer.Active);
            Assert.Equal("fakeauthid", fakeAuthFacade.CustomerAuthId);
            Assert.Equal(customerId, fakeOrderFacade.CustomerId);
            Assert.Null(fakeOrderFacade.Customer);
            Assert.Equal(customerId, fakeReviewFacade.CustomerId);
            Assert.Null(fakeReviewFacade.Customer);
        }

        [Fact]
        public async void DeleteExistingCustomer_CustomerNotActive_VerifyMocks()
        {
            //Arrange
            DefaultSetup();
            SetMockCustomerRepo(customerActive: false);
            controller = new CustomerManagementController(config, logger, mockRepo.Object, mapper,
                mockOrderFacade.Object, mockReviewFacade.Object, mockAuthFacade.Object);
            SetupUser(controller);

            //Act
            var result = await controller.Delete(1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.MatchingAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Once);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Once);
            mockReviewFacade.Verify(facade => facade.NewCustomer(It.IsAny<ReviewCustomerDto>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.EditCustomer(It.IsAny<ReviewCustomerDto>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Once);
            mockAuthFacade.Verify(facade => facade.DeleteAccount(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async void DeleteExistingCustomer_RepoFails_ShouldNotFound()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.autoFails = true;
            int customerId = 1;

            //Act
            var result = await controller.Delete(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            Assert.Equal(customerRepoModel.GivenName, fakeRepo.Customer.GivenName);
            Assert.Equal(customerRepoModel.FamilyName, fakeRepo.Customer.FamilyName);
            Assert.Equal(customerRepoModel.AddressOne, fakeRepo.Customer.AddressOne);
            Assert.Equal(customerRepoModel.AddressTwo, fakeRepo.Customer.AddressTwo);
            Assert.Equal(customerRepoModel.Town, fakeRepo.Customer.Town);
            Assert.Equal(customerRepoModel.State, fakeRepo.Customer.State);
            Assert.Equal(customerRepoModel.AreaCode, fakeRepo.Customer.AreaCode);
            Assert.Equal(customerRepoModel.Country, fakeRepo.Customer.Country);
            Assert.Equal(customerRepoModel.EmailAddress, fakeRepo.Customer.EmailAddress);
            Assert.Equal(customerRepoModel.TelephoneNumber, fakeRepo.Customer.TelephoneNumber);
            Assert.True(customerRepoModel.RequestedDeletion == fakeRepo.Customer.RequestedDeletion);
            Assert.True(customerRepoModel.CanPurchase == fakeRepo.Customer.CanPurchase);
            Assert.True(customerRepoModel.Active == fakeRepo.Customer.Active);
            Assert.Null(fakeAuthFacade.CustomerAuthId);
            Assert.Equal(0, fakeOrderFacade.CustomerId);
            Assert.Null(fakeOrderFacade.Customer);
            Assert.Equal(0, fakeReviewFacade.CustomerId);
            Assert.Null(fakeReviewFacade.Customer);
        }
    }
}
