using AspNet.Security.OpenIdConnect.Primitives;
using AutoMapper;
using Customer.AccountAPI;
using Customer.AccountAPI.Controllers;
using Customer.AccountAPI.Models;
using Customer.OrderFacade;
using Customer.OrderFacade.Models;
using Customer.Repository;
using Customer.Repository.Models;
using Customer.ReviewFacade;
using Customer.ReviewFacade.Models;
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
    public class CustomerControllerUnitTests
    {

        private CustomerDto customerDto;
        private CustomerRepoModel customerRepoModel;
        private FakeCustomerRepository fakeRepo;
        private Mock<ICustomerRepository> mockRepo;
        private FakeOrderFacade fakeOrderFacade;
        private FakeReviewCustomerFacade fakeReviewFacade;
        private Mock<IOrderFacade> mockOrderFacade;
        private Mock<IReviewCustomerFacade> mockReviewFacade;
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
                .CreateLogger<CustomerController>();
        }

        private void SetMockCustomerRepo(bool customerExists = true, bool customerActive = true, bool succeeds = true, bool authMatch = true)
        {
            mockRepo = new Mock<ICustomerRepository>(MockBehavior.Strict);
            mockRepo.Setup(repo => repo.CustomerExists(It.IsAny<int>())).ReturnsAsync(customerExists).Verifiable();
            mockRepo.Setup(repo => repo.IsCustomerActive(It.IsAny<int>())).ReturnsAsync(customerActive).Verifiable();
            mockRepo.Setup(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>())).ReturnsAsync(customerDto.CustomerId).Verifiable();
            mockRepo.Setup(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>())).ReturnsAsync(succeeds).Verifiable();
            mockRepo.Setup(repo => repo.MatchingAuthId(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(authMatch).Verifiable();
            mockRepo.Setup(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>())).ReturnsAsync(succeeds).Verifiable();
            if (succeeds)
            {
                mockRepo.Setup(repo => repo.GetCustomer(It.IsAny<int>())).ReturnsAsync(customerRepoModel).Verifiable();
            }
            else
            {
                mockRepo.Setup(repo => repo.GetCustomer(It.IsAny<int>())).ReturnsAsync((CustomerRepoModel)null).Verifiable();
            }
        }

        private void SetMockOrderFacade(bool customerExists = true, bool succeeds = true)
        {
            mockOrderFacade = new Mock<IOrderFacade>(MockBehavior.Strict);
            mockOrderFacade.Setup(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>())).ReturnsAsync(succeeds).Verifiable();
            mockOrderFacade.Setup(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>())).ReturnsAsync(succeeds).Verifiable();
            mockOrderFacade.Setup(facade => facade.DeleteCustomer(It.IsAny<int>())).ReturnsAsync(succeeds).Verifiable();
        }

        private void SetMockReviewFacade(bool customerExists = true, bool succeeds = true)
        {
            mockReviewFacade = new Mock<IReviewCustomerFacade>(MockBehavior.Strict);
            mockReviewFacade.Setup(facade => facade.NewCustomer(It.IsAny<ReviewCustomerDto>())).ReturnsAsync(succeeds).Verifiable();
            mockReviewFacade.Setup(facade => facade.EditCustomer(It.IsAny<ReviewCustomerDto>())).ReturnsAsync(succeeds).Verifiable();
            mockReviewFacade.Setup(facade => facade.DeleteCustomer(It.IsAny<int>())).ReturnsAsync(succeeds).Verifiable();
        }

        private void SetupUser(CustomerController controller)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "name"),
                                        new Claim(ClaimTypes.Name, "name"),
                                        new Claim(OpenIdConnectConstants.Claims.Subject, "fakeauthid" ),
                                        new Claim("client_id","customer_web_app")
                                   }, "TestAuth"));
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
        }

        private void SetupApi(CustomerController controller)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim("client_id","customer_ordering_api")
                                   }, "TestAuth"));
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
        }

        private void SetFakeOrderFacade()
        {
            fakeOrderFacade = new FakeOrderFacade();
        }

        private void SetFakeReviewFacade()
        {
            fakeReviewFacade = new FakeReviewCustomerFacade();
        }

        private void DefaultSetup(bool withMocks = false, bool setupUser = true, bool setupApi = false)
        {
            SetStandardCustomerDto();
            SetStandardCustomerRepoModel();
            SetFakeRepo(customerRepoModel);
            SetFakeOrderFacade();
            SetFakeReviewFacade();
            SetMapper();
            SetLogger();
            SetMockCustomerRepo();
            SetMockOrderFacade();
            SetMockReviewFacade();
            if (!withMocks)
            {
                controller = new CustomerController(logger, fakeRepo, mapper, fakeOrderFacade, fakeReviewFacade);
            }
            else
            {
                controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
            }
            if (setupUser)
            {
                SetupUser(controller);
            }
            if (setupApi)
            {
                SetupApi(controller);
            }
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
            DefaultSetup(withMocks: true);
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
            DefaultSetup(withMocks: true);
            SetMockCustomerRepo(customerExists: false, customerActive: true, succeeds: true);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
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
            DefaultSetup(withMocks: true);
            SetMockCustomerRepo(customerExists: true, customerActive: false, succeeds: true);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
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
            DefaultSetup(withMocks: true);
            SetMockCustomerRepo();
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
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

        [Fact]
        public async void GetCustomer_RepoFailure_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockCustomerRepo(succeeds: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
            SetupUser(controller);
            int customerId = 1;
            customerRepoModel.CustomerAuthId = "officialId";

            //Act
            var result = await controller.Get(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerId), Times.Once);
            mockRepo.Verify(repo => repo.GetCustomer(customerId), Times.Once);
        }

        [Fact]
        public async void GetCustomer_NoUser_ShouldForbid()
        {
            //Arrange
            DefaultSetup(setupUser: false);

            //Act
            var result = await controller.Get(1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void GetCustomer_NoUser_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, setupUser: false);
            SetMockCustomerRepo();
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
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

        [Fact]
        public async void PostNewCustomer_ShouldOk()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer = null;

            //Act
            var result = await controller.Post(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Customer);
            Assert.Equal(fakeRepo.Customer.CustomerId, customerDto.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, customerDto.CustomerAuthId);
            Assert.Equal(fakeRepo.Customer.GivenName, customerDto.GivenName);
            Assert.Equal(fakeRepo.Customer.FamilyName, customerDto.FamilyName);
            Assert.Equal(fakeRepo.Customer.AddressOne, customerDto.AddressOne);
            Assert.Equal(fakeRepo.Customer.AddressTwo, customerDto.AddressTwo);
            Assert.Equal(fakeRepo.Customer.Town, customerDto.Town);
            Assert.Equal(fakeRepo.Customer.State, customerDto.State);
            Assert.Equal(fakeRepo.Customer.AreaCode, customerDto.AreaCode);
            Assert.Equal(fakeRepo.Customer.Country, customerDto.Country);
            Assert.Equal(fakeRepo.Customer.EmailAddress, customerDto.EmailAddress);
            Assert.Equal(fakeRepo.Customer.TelephoneNumber, customerDto.TelephoneNumber);
            Assert.Equal(fakeRepo.Customer.RequestedDeletion, customerDto.RequestedDeletion);
            Assert.Equal(fakeRepo.Customer.CanPurchase, customerDto.CanPurchase);
            Assert.Equal(fakeRepo.Customer.Active, customerDto.Active);
        }

        [Fact]
        public async void PostNewCustomer_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockCustomerRepo(customerExists: false, customerActive: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
            SetupUser(controller);

            //Act
            var result = await controller.Post(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Once);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Once);
        }

        [Fact]
        public async void PostNewCustomer_CustomerAlreadyExists_ShouldOkWithEditedDetails()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await controller.Post(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Customer);
            Assert.Equal(fakeRepo.Customer.CustomerId, customerDto.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, customerDto.CustomerAuthId);
            Assert.Equal(fakeRepo.Customer.GivenName, customerDto.GivenName);
            Assert.Equal(fakeRepo.Customer.FamilyName, customerDto.FamilyName);
            Assert.Equal(fakeRepo.Customer.AddressOne, customerDto.AddressOne);
            Assert.Equal(fakeRepo.Customer.AddressTwo, customerDto.AddressTwo);
            Assert.Equal(fakeRepo.Customer.Town, customerDto.Town);
            Assert.Equal(fakeRepo.Customer.State, customerDto.State);
            Assert.Equal(fakeRepo.Customer.AreaCode, customerDto.AreaCode);
            Assert.Equal(fakeRepo.Customer.Country, customerDto.Country);
            Assert.Equal(fakeRepo.Customer.EmailAddress, customerDto.EmailAddress);
            Assert.Equal(fakeRepo.Customer.TelephoneNumber, customerDto.TelephoneNumber);
            Assert.Equal(fakeRepo.Customer.RequestedDeletion, customerDto.RequestedDeletion);
            Assert.Equal(fakeRepo.Customer.CanPurchase, customerDto.CanPurchase);
            Assert.Equal(fakeRepo.Customer.Active, customerDto.Active);
        }

        [Fact]
        public async void PostNewCustomer_CustomerAlreadyExists_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockCustomerRepo();
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
            SetupUser(controller);

            //Act
            var result = await controller.Post(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Once);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Once);
        }

        [Fact]
        public async void PostNewCustomer_CustomerAlreadyExistsAndInactive_ShouldNotFound()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer.Active = false;
            var editedCustomer = GetEditedDetailsDto();

            //Act
            var result = await controller.Post(editedCustomer);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Customer);
            Assert.Equal(fakeRepo.Customer.CustomerId, editedCustomer.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, editedCustomer.CustomerAuthId);
            Assert.NotEqual(fakeRepo.Customer.GivenName, editedCustomer.GivenName);
            Assert.NotEqual(fakeRepo.Customer.FamilyName, editedCustomer.FamilyName);
            Assert.NotEqual(fakeRepo.Customer.AddressOne, editedCustomer.AddressOne);
            Assert.NotEqual(fakeRepo.Customer.AddressTwo, editedCustomer.AddressTwo);
            Assert.NotEqual(fakeRepo.Customer.Town, editedCustomer.Town);
            Assert.NotEqual(fakeRepo.Customer.State, editedCustomer.State);
            Assert.NotEqual(fakeRepo.Customer.AreaCode, editedCustomer.AreaCode);
            Assert.NotEqual(fakeRepo.Customer.Country, editedCustomer.Country);
            Assert.NotEqual(fakeRepo.Customer.EmailAddress, editedCustomer.EmailAddress);
            Assert.NotEqual(fakeRepo.Customer.TelephoneNumber, editedCustomer.TelephoneNumber);
            Assert.Equal(fakeRepo.Customer.RequestedDeletion, editedCustomer.RequestedDeletion);
            Assert.Equal(fakeRepo.Customer.CanPurchase, editedCustomer.CanPurchase);
            Assert.NotEqual(fakeRepo.Customer.Active, editedCustomer.Active);
        }

        [Fact]
        public async void PostNewCustomer_CustomerAlreadyExistsAndInactive_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockCustomerRepo(customerActive: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
            SetupUser(controller);
            var editedCustomer = GetEditedDetailsDto();

            //Act
            var result = await controller.Post(editedCustomer);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
        }

        [Fact]
        public async void PostNewCustomer_AuthIdDoesntMatch_ShouldForbid()
        {
            //Arrange
            DefaultSetup();
            var editedCustomer = GetEditedDetailsDto();
            editedCustomer.CustomerAuthId = "differentId";

            //Act
            var result = await controller.Post(editedCustomer);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Customer);
            Assert.Equal(fakeRepo.Customer.CustomerId, editedCustomer.CustomerId);
            Assert.NotEqual(fakeRepo.Customer.CustomerAuthId, editedCustomer.CustomerAuthId);
            Assert.NotEqual(fakeRepo.Customer.GivenName, editedCustomer.GivenName);
            Assert.NotEqual(fakeRepo.Customer.FamilyName, editedCustomer.FamilyName);
            Assert.NotEqual(fakeRepo.Customer.AddressOne, editedCustomer.AddressOne);
            Assert.NotEqual(fakeRepo.Customer.AddressTwo, editedCustomer.AddressTwo);
            Assert.NotEqual(fakeRepo.Customer.Town, editedCustomer.Town);
            Assert.NotEqual(fakeRepo.Customer.State, editedCustomer.State);
            Assert.NotEqual(fakeRepo.Customer.AreaCode, editedCustomer.AreaCode);
            Assert.NotEqual(fakeRepo.Customer.Country, editedCustomer.Country);
            Assert.NotEqual(fakeRepo.Customer.EmailAddress, editedCustomer.EmailAddress);
            Assert.NotEqual(fakeRepo.Customer.TelephoneNumber, editedCustomer.TelephoneNumber);
            Assert.Equal(fakeRepo.Customer.RequestedDeletion, editedCustomer.RequestedDeletion);
            Assert.Equal(fakeRepo.Customer.CanPurchase, editedCustomer.CanPurchase);
            Assert.Equal(fakeRepo.Customer.Active, editedCustomer.Active);
        }

        [Fact]
        public async void PostNewCustomer_AuthIdDoesntMatch_CheckMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockCustomerRepo(authMatch: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
            SetupUser(controller);
            var editedCustomer = GetEditedDetailsDto();
            editedCustomer.CustomerAuthId = "differentId";

            //Act
            var result = await controller.Post(editedCustomer);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.MatchingAuthId(editedCustomer.CustomerId, editedCustomer.CustomerAuthId), Times.Never);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
        }

        [Fact]
        public async void PostNewCustomer_NullCustomer_ShouldUnprocessableEntity()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await controller.Post(null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PostNewCustomer_NullCustomer_CheckMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockCustomerRepo(authMatch: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
            SetupUser(controller);
            var editedCustomer = GetEditedDetailsDto();

            //Act
            var result = await controller.Post(null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.MatchingAuthId(editedCustomer.CustomerId, editedCustomer.CustomerAuthId), Times.Never);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
        }

        [Fact]
        public async void PostNewCustomer_NoUser_ShouldForbid()
        {
            //Arrange
            DefaultSetup(setupUser: false);

            //Act
            var result = await controller.Post(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
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
        }

        [Fact]
        public async void PostNewCustomer_NoUser_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, setupUser: false);
            SetMockCustomerRepo(customerActive: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
            SetupUser(controller);

            //Act
            var result = await controller.Post(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
        }

        [Fact]
        public async void EditCustomer_CustomerDoesntExist_ShouldOkCreatingNewCustomer()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer = null;

            //Act
            var result = await controller.Put(customerDto.CustomerId, customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Customer);
            Assert.Equal(fakeRepo.Customer.CustomerId, customerDto.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, customerDto.CustomerAuthId);
            Assert.Equal(fakeRepo.Customer.GivenName, customerDto.GivenName);
            Assert.Equal(fakeRepo.Customer.FamilyName, customerDto.FamilyName);
            Assert.Equal(fakeRepo.Customer.AddressOne, customerDto.AddressOne);
            Assert.Equal(fakeRepo.Customer.AddressTwo, customerDto.AddressTwo);
            Assert.Equal(fakeRepo.Customer.Town, customerDto.Town);
            Assert.Equal(fakeRepo.Customer.State, customerDto.State);
            Assert.Equal(fakeRepo.Customer.AreaCode, customerDto.AreaCode);
            Assert.Equal(fakeRepo.Customer.Country, customerDto.Country);
            Assert.Equal(fakeRepo.Customer.EmailAddress, customerDto.EmailAddress);
            Assert.Equal(fakeRepo.Customer.TelephoneNumber, customerDto.TelephoneNumber);
            Assert.Equal(fakeRepo.Customer.RequestedDeletion, customerDto.RequestedDeletion);
            Assert.Equal(fakeRepo.Customer.CanPurchase, customerDto.CanPurchase);
            Assert.Equal(fakeRepo.Customer.Active, customerDto.Active);
        }

        [Fact]
        public async void EditCustomer_CustomerDoesntExist_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockCustomerRepo(customerExists: false, customerActive: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
            SetupUser(controller);

            //Act
            var result = await controller.Put(customerDto.CustomerId, customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Once);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Once);
        }

        [Fact]
        public async void EditCustomer__ShouldOkWithEditedDetails()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await controller.Put(customerDto.CustomerId, customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Customer);
            Assert.Equal(fakeRepo.Customer.CustomerId, customerDto.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, customerDto.CustomerAuthId);
            Assert.Equal(fakeRepo.Customer.GivenName, customerDto.GivenName);
            Assert.Equal(fakeRepo.Customer.FamilyName, customerDto.FamilyName);
            Assert.Equal(fakeRepo.Customer.AddressOne, customerDto.AddressOne);
            Assert.Equal(fakeRepo.Customer.AddressTwo, customerDto.AddressTwo);
            Assert.Equal(fakeRepo.Customer.Town, customerDto.Town);
            Assert.Equal(fakeRepo.Customer.State, customerDto.State);
            Assert.Equal(fakeRepo.Customer.AreaCode, customerDto.AreaCode);
            Assert.Equal(fakeRepo.Customer.Country, customerDto.Country);
            Assert.Equal(fakeRepo.Customer.EmailAddress, customerDto.EmailAddress);
            Assert.Equal(fakeRepo.Customer.TelephoneNumber, customerDto.TelephoneNumber);
            Assert.Equal(fakeRepo.Customer.RequestedDeletion, customerDto.RequestedDeletion);
            Assert.Equal(fakeRepo.Customer.CanPurchase, customerDto.CanPurchase);
            Assert.Equal(fakeRepo.Customer.Active, customerDto.Active);
        }

        [Fact]
        public async void EditCustomer_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockCustomerRepo();
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
            SetupUser(controller);

            //Act
            var result = await controller.Put(customerDto.CustomerId, customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Once);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Once);
        }

        [Fact]
        public async void EditCustomer_CustomerIsInactive_ShouldNotFound()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer.Active = false;
            var editedCustomer = GetEditedDetailsDto();

            //Act
            var result = await controller.Put(customerDto.CustomerId, customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Customer);
            Assert.Equal(fakeRepo.Customer.CustomerId, editedCustomer.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, editedCustomer.CustomerAuthId);
            Assert.NotEqual(fakeRepo.Customer.GivenName, editedCustomer.GivenName);
            Assert.NotEqual(fakeRepo.Customer.FamilyName, editedCustomer.FamilyName);
            Assert.NotEqual(fakeRepo.Customer.AddressOne, editedCustomer.AddressOne);
            Assert.NotEqual(fakeRepo.Customer.AddressTwo, editedCustomer.AddressTwo);
            Assert.NotEqual(fakeRepo.Customer.Town, editedCustomer.Town);
            Assert.NotEqual(fakeRepo.Customer.State, editedCustomer.State);
            Assert.NotEqual(fakeRepo.Customer.AreaCode, editedCustomer.AreaCode);
            Assert.NotEqual(fakeRepo.Customer.Country, editedCustomer.Country);
            Assert.NotEqual(fakeRepo.Customer.EmailAddress, editedCustomer.EmailAddress);
            Assert.NotEqual(fakeRepo.Customer.TelephoneNumber, editedCustomer.TelephoneNumber);
            Assert.Equal(fakeRepo.Customer.RequestedDeletion, editedCustomer.RequestedDeletion);
            Assert.Equal(fakeRepo.Customer.CanPurchase, editedCustomer.CanPurchase);
            Assert.NotEqual(fakeRepo.Customer.Active, editedCustomer.Active);
        }

        [Fact]
        public async void EditCustomer_CustomerIsInactive_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockCustomerRepo(customerActive: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
            SetupUser(controller);
            var editedCustomer = GetEditedDetailsDto();

            //Act
            var result = await controller.Put(customerDto.CustomerId, customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
        }

        [Fact]
        public async void EditCustomer_AuthIdDoesntMatch_ShouldForbid()
        {
            //Arrange
            DefaultSetup();
            var editedCustomer = GetEditedDetailsDto();
            editedCustomer.CustomerAuthId = "differentId";

            //Act
            var result = await controller.Put(customerDto.CustomerId, editedCustomer);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Customer);
            Assert.Equal(fakeRepo.Customer.CustomerId, editedCustomer.CustomerId);
            Assert.NotEqual(fakeRepo.Customer.CustomerAuthId, editedCustomer.CustomerAuthId);
            Assert.NotEqual(fakeRepo.Customer.GivenName, editedCustomer.GivenName);
            Assert.NotEqual(fakeRepo.Customer.FamilyName, editedCustomer.FamilyName);
            Assert.NotEqual(fakeRepo.Customer.AddressOne, editedCustomer.AddressOne);
            Assert.NotEqual(fakeRepo.Customer.AddressTwo, editedCustomer.AddressTwo);
            Assert.NotEqual(fakeRepo.Customer.Town, editedCustomer.Town);
            Assert.NotEqual(fakeRepo.Customer.State, editedCustomer.State);
            Assert.NotEqual(fakeRepo.Customer.AreaCode, editedCustomer.AreaCode);
            Assert.NotEqual(fakeRepo.Customer.Country, editedCustomer.Country);
            Assert.NotEqual(fakeRepo.Customer.EmailAddress, editedCustomer.EmailAddress);
            Assert.NotEqual(fakeRepo.Customer.TelephoneNumber, editedCustomer.TelephoneNumber);
            Assert.Equal(fakeRepo.Customer.RequestedDeletion, editedCustomer.RequestedDeletion);
            Assert.Equal(fakeRepo.Customer.CanPurchase, editedCustomer.CanPurchase);
            Assert.Equal(fakeRepo.Customer.Active, editedCustomer.Active);
        }

        [Fact]
        public async void EditCustomer_AuthIdDoesntMatch_CheckMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockCustomerRepo(authMatch: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
            SetupUser(controller);
            var editedCustomer = GetEditedDetailsDto();
            customerDto.CustomerAuthId = "differentId";

            //Act
            var result = await controller.Put(customerDto.CustomerId, customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.MatchingAuthId(editedCustomer.CustomerId, editedCustomer.CustomerAuthId), Times.Never);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
        }

        [Fact]
        public async void EditCustomer_NullCustomer_ShouldUnprocessableEntity()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await controller.Put(1, null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void EditCustomer_NullCustomer_CheckMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockCustomerRepo(authMatch: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
            SetupUser(controller);
            var editedCustomer = GetEditedDetailsDto();

            //Act
            var result = await controller.Put(1, null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.MatchingAuthId(editedCustomer.CustomerId, editedCustomer.CustomerAuthId), Times.Never);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
        }

        [Fact]
        public async void EditCustomer_NoUser_ShouldForbid()
        {
            //Arrange
            DefaultSetup(setupUser: false);

            //Act
            var result = await controller.Put(1, customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
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
        }

        [Fact]
        public async void EditNewCustomer_NoUser_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, setupUser: false);
            SetMockCustomerRepo(customerActive: true);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);

            //Act
            var result = await controller.Put(1, customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
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
        }

        [Fact]
        public async void DeleteExistingCustomer_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            int customerId = 1;

            //Act
            var result = await controller.Delete(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.MatchingAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Once);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Once);
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
        }

        [Fact]
        public async void DeleteExistingCustomer_CustomerDoesntExist_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockCustomerRepo(customerExists: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
            SetupUser(controller);
            int customerId = 2;

            //Act
            var result = await controller.Delete(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.MatchingAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void DeleteExistingCustomer_CustomerNotActive_ShouldOk()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer.Active = false;

            //Act
            var result = await controller.Delete(1);

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
        }

        [Fact]
        public async void DeleteExistingCustomer_CustomerNotActive_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockCustomerRepo(customerActive: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
            SetupUser(controller);

            //Act
            var result = await controller.Delete(1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.MatchingAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Once);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Once);
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
        }

        [Fact]
        public async void DeleteExistingCustomer_RepoFails_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockCustomerRepo(authMatch: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
            SetupUser(controller);
            int customerId = 1;

            //Act
            var result = await controller.Delete(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.MatchingAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void DeleteExistingCustomer_NoUser_ShouldForbid()
        {
            //Arrange
            DefaultSetup(setupUser: false);

            //Act
            var result = await controller.Delete(1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
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
        }

        [Fact]
        public async void DeleteExistingCustomer_NoUser_VerifyMocks()
        {
            //Arrange
            DefaultSetup(setupUser: false, withMocks: true);
            SetMockCustomerRepo(customerActive: true);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);

            //Act
            var result = await controller.Delete(1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.MatchingAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void GetExistingCustomer_FromOrderingApi_ShouldOk()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true);

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
        public async void GetExistingCustomer_FromOrderingApi_VerifyMockCalls()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true, withMocks: true);
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
            mockRepo.Verify(repo => repo.CustomerExists(customerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerId), Times.Once);
            mockRepo.Verify(repo => repo.GetCustomer(customerId), Times.Once);
        }

        [Fact]
        public async void GetCustomer_DoesntExist_FromOrderingApi_ShouldNotFound()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true);

            //Act
            var result = await controller.Get(2);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void GetCustomer_DoesntExist_FromOrderingApi_VerifyMocks()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true, withMocks: true);
            SetMockCustomerRepo(customerExists: false, customerActive: true, succeeds: true);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
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
        public async void GetCustomer_NotActive_FromOrderingApi_ShouldNotFound()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true);
            customerRepoModel.Active = false;

            //Act
            var result = await controller.Get(1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void GetCustomer_NotActive_FromOrderingApi_VerifyMocks()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true, withMocks: true);
            SetMockCustomerRepo(customerExists: true, customerActive: false, succeeds: true);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
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
        public async void GetCustomer_InvaildTokenId_FromOrderingApi_VerifyMocks()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true, withMocks: true);
            SetMockCustomerRepo();
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
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

        [Fact]
        public async void GetCustomer_RepoFailure_FromOrderingApi_VerifyMocks()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true, withMocks: true);
            SetMockCustomerRepo(succeeds: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
            SetupUser(controller);
            int customerId = 1;
            customerRepoModel.CustomerAuthId = "officialId";

            //Act
            var result = await controller.Get(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerId), Times.Once);
            mockRepo.Verify(repo => repo.GetCustomer(customerId), Times.Once);
        }

        [Fact]
        public async void PostNewCustomer_FromOrderingApi_ShouldOk()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true);
            fakeRepo.Customer = null;

            //Act
            var result = await controller.Post(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Customer);
            Assert.Equal(fakeRepo.Customer.CustomerId, customerDto.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, customerDto.CustomerAuthId);
            Assert.Equal(fakeRepo.Customer.GivenName, customerDto.GivenName);
            Assert.Equal(fakeRepo.Customer.FamilyName, customerDto.FamilyName);
            Assert.Equal(fakeRepo.Customer.AddressOne, customerDto.AddressOne);
            Assert.Equal(fakeRepo.Customer.AddressTwo, customerDto.AddressTwo);
            Assert.Equal(fakeRepo.Customer.Town, customerDto.Town);
            Assert.Equal(fakeRepo.Customer.State, customerDto.State);
            Assert.Equal(fakeRepo.Customer.AreaCode, customerDto.AreaCode);
            Assert.Equal(fakeRepo.Customer.Country, customerDto.Country);
            Assert.Equal(fakeRepo.Customer.EmailAddress, customerDto.EmailAddress);
            Assert.Equal(fakeRepo.Customer.TelephoneNumber, customerDto.TelephoneNumber);
            Assert.Equal(fakeRepo.Customer.RequestedDeletion, customerDto.RequestedDeletion);
            Assert.Equal(fakeRepo.Customer.CanPurchase, customerDto.CanPurchase);
            Assert.Equal(fakeRepo.Customer.Active, customerDto.Active);
        }

        [Fact]
        public async void PostNewCustomer_FromOrderingApi_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, setupUser: false, setupApi: true);
            SetMockCustomerRepo(customerExists: false, customerActive: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
            SetupUser(controller);

            //Act
            var result = await controller.Post(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Once);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Once);
        }

        [Fact]
        public async void PostNewCustomer_CustomerAlreadyExists_FromOrderingApi_ShouldOkWithEditedDetails()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true);

            //Act
            var result = await controller.Post(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Customer);
            Assert.Equal(fakeRepo.Customer.CustomerId, customerDto.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, customerDto.CustomerAuthId);
            Assert.Equal(fakeRepo.Customer.GivenName, customerDto.GivenName);
            Assert.Equal(fakeRepo.Customer.FamilyName, customerDto.FamilyName);
            Assert.Equal(fakeRepo.Customer.AddressOne, customerDto.AddressOne);
            Assert.Equal(fakeRepo.Customer.AddressTwo, customerDto.AddressTwo);
            Assert.Equal(fakeRepo.Customer.Town, customerDto.Town);
            Assert.Equal(fakeRepo.Customer.State, customerDto.State);
            Assert.Equal(fakeRepo.Customer.AreaCode, customerDto.AreaCode);
            Assert.Equal(fakeRepo.Customer.Country, customerDto.Country);
            Assert.Equal(fakeRepo.Customer.EmailAddress, customerDto.EmailAddress);
            Assert.Equal(fakeRepo.Customer.TelephoneNumber, customerDto.TelephoneNumber);
            Assert.Equal(fakeRepo.Customer.RequestedDeletion, customerDto.RequestedDeletion);
            Assert.Equal(fakeRepo.Customer.CanPurchase, customerDto.CanPurchase);
            Assert.Equal(fakeRepo.Customer.Active, customerDto.Active);
        }

        [Fact]
        public async void PostNewCustomer_CustomerAlreadyExists_FromOrderingApi_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, setupUser: false, setupApi: true);
            SetMockCustomerRepo();
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
            SetupUser(controller);

            //Act
            var result = await controller.Post(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Once);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Once);
        }

        [Fact]
        public async void PostNewCustomer_CustomerAlreadyExistsAndInactive_FromOrderingApi_ShouldNotFound()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true);
            fakeRepo.Customer.Active = false;
            var editedCustomer = GetEditedDetailsDto();

            //Act
            var result = await controller.Post(editedCustomer);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Customer);
            Assert.Equal(fakeRepo.Customer.CustomerId, editedCustomer.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, editedCustomer.CustomerAuthId);
            Assert.NotEqual(fakeRepo.Customer.GivenName, editedCustomer.GivenName);
            Assert.NotEqual(fakeRepo.Customer.FamilyName, editedCustomer.FamilyName);
            Assert.NotEqual(fakeRepo.Customer.AddressOne, editedCustomer.AddressOne);
            Assert.NotEqual(fakeRepo.Customer.AddressTwo, editedCustomer.AddressTwo);
            Assert.NotEqual(fakeRepo.Customer.Town, editedCustomer.Town);
            Assert.NotEqual(fakeRepo.Customer.State, editedCustomer.State);
            Assert.NotEqual(fakeRepo.Customer.AreaCode, editedCustomer.AreaCode);
            Assert.NotEqual(fakeRepo.Customer.Country, editedCustomer.Country);
            Assert.NotEqual(fakeRepo.Customer.EmailAddress, editedCustomer.EmailAddress);
            Assert.NotEqual(fakeRepo.Customer.TelephoneNumber, editedCustomer.TelephoneNumber);
            Assert.Equal(fakeRepo.Customer.RequestedDeletion, editedCustomer.RequestedDeletion);
            Assert.Equal(fakeRepo.Customer.CanPurchase, editedCustomer.CanPurchase);
            Assert.NotEqual(fakeRepo.Customer.Active, editedCustomer.Active);
        }

        [Fact]
        public async void PostNewCustomer_CustomerAlreadyExistsAndInactive_FromOrderingApi_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, setupUser: false, setupApi: true);
            SetMockCustomerRepo(customerActive: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
            SetupUser(controller);
            var editedCustomer = GetEditedDetailsDto();

            //Act
            var result = await controller.Post(editedCustomer);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
        }


        [Fact]
        public async void PostNewCustomer_AuthIdDoesntMatch_FromOrderingApi_CheckMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, setupUser: false, setupApi: true);
            SetMockCustomerRepo(authMatch: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
            SetupUser(controller);
            var editedCustomer = GetEditedDetailsDto();
            editedCustomer.CustomerAuthId = "differentId";

            //Act
            var result = await controller.Post(editedCustomer);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.MatchingAuthId(editedCustomer.CustomerId, editedCustomer.CustomerAuthId), Times.Never);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
        }

        [Fact]
        public async void PostNewCustomer_NullCustomer_FromOrderingApi_ShouldUnprocessableEntity()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true);

            //Act
            var result = await controller.Post(null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PostNewCustomer_NullCustomer_FromOrderingApi_CheckMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, setupUser: false, setupApi: true);
            SetMockCustomerRepo(authMatch: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
            SetupUser(controller);
            var editedCustomer = GetEditedDetailsDto();

            //Act
            var result = await controller.Post(null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.MatchingAuthId(editedCustomer.CustomerId, editedCustomer.CustomerAuthId), Times.Never);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
        }

        [Fact]
        public async void EditCustomer_CustomerDoesntExist_FromOrderingApi_ShouldOkCreatingNewCustomer()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true);
            fakeRepo.Customer = null;

            //Act
            var result = await controller.Put(customerDto.CustomerId, customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Customer);
            Assert.Equal(fakeRepo.Customer.CustomerId, customerDto.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, customerDto.CustomerAuthId);
            Assert.Equal(fakeRepo.Customer.GivenName, customerDto.GivenName);
            Assert.Equal(fakeRepo.Customer.FamilyName, customerDto.FamilyName);
            Assert.Equal(fakeRepo.Customer.AddressOne, customerDto.AddressOne);
            Assert.Equal(fakeRepo.Customer.AddressTwo, customerDto.AddressTwo);
            Assert.Equal(fakeRepo.Customer.Town, customerDto.Town);
            Assert.Equal(fakeRepo.Customer.State, customerDto.State);
            Assert.Equal(fakeRepo.Customer.AreaCode, customerDto.AreaCode);
            Assert.Equal(fakeRepo.Customer.Country, customerDto.Country);
            Assert.Equal(fakeRepo.Customer.EmailAddress, customerDto.EmailAddress);
            Assert.Equal(fakeRepo.Customer.TelephoneNumber, customerDto.TelephoneNumber);
            Assert.Equal(fakeRepo.Customer.RequestedDeletion, customerDto.RequestedDeletion);
            Assert.Equal(fakeRepo.Customer.CanPurchase, customerDto.CanPurchase);
            Assert.Equal(fakeRepo.Customer.Active, customerDto.Active);
        }

        [Fact]
        public async void EditCustomer_CustomerDoesntExist_FromOrderingApi_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, setupUser: false, setupApi: true);
            SetMockCustomerRepo(customerExists: false, customerActive: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
            SetupUser(controller);

            //Act
            var result = await controller.Put(customerDto.CustomerId, customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Once);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Once);
        }

        [Fact]
        public async void EditCustomer__FromOrderingApi_ShouldOkWithEditedDetails()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true);

            //Act
            var result = await controller.Put(customerDto.CustomerId, customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Customer);
            Assert.Equal(fakeRepo.Customer.CustomerId, customerDto.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, customerDto.CustomerAuthId);
            Assert.Equal(fakeRepo.Customer.GivenName, customerDto.GivenName);
            Assert.Equal(fakeRepo.Customer.FamilyName, customerDto.FamilyName);
            Assert.Equal(fakeRepo.Customer.AddressOne, customerDto.AddressOne);
            Assert.Equal(fakeRepo.Customer.AddressTwo, customerDto.AddressTwo);
            Assert.Equal(fakeRepo.Customer.Town, customerDto.Town);
            Assert.Equal(fakeRepo.Customer.State, customerDto.State);
            Assert.Equal(fakeRepo.Customer.AreaCode, customerDto.AreaCode);
            Assert.Equal(fakeRepo.Customer.Country, customerDto.Country);
            Assert.Equal(fakeRepo.Customer.EmailAddress, customerDto.EmailAddress);
            Assert.Equal(fakeRepo.Customer.TelephoneNumber, customerDto.TelephoneNumber);
            Assert.Equal(fakeRepo.Customer.RequestedDeletion, customerDto.RequestedDeletion);
            Assert.Equal(fakeRepo.Customer.CanPurchase, customerDto.CanPurchase);
            Assert.Equal(fakeRepo.Customer.Active, customerDto.Active);
        }

        [Fact]
        public async void EditCustomer_FromOrderingApi_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, setupUser: false, setupApi: true);
            SetMockCustomerRepo();
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
            SetupUser(controller);

            //Act
            var result = await controller.Put(customerDto.CustomerId, customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Once);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Once);
        }

        [Fact]
        public async void EditCustomer_CustomerIsInactive_FromOrderingApi_ShouldNotFound()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true);
            fakeRepo.Customer.Active = false;
            var editedCustomer = GetEditedDetailsDto();

            //Act
            var result = await controller.Put(customerDto.CustomerId, customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Customer);
            Assert.Equal(fakeRepo.Customer.CustomerId, editedCustomer.CustomerId);
            Assert.Equal(fakeRepo.Customer.CustomerAuthId, editedCustomer.CustomerAuthId);
            Assert.NotEqual(fakeRepo.Customer.GivenName, editedCustomer.GivenName);
            Assert.NotEqual(fakeRepo.Customer.FamilyName, editedCustomer.FamilyName);
            Assert.NotEqual(fakeRepo.Customer.AddressOne, editedCustomer.AddressOne);
            Assert.NotEqual(fakeRepo.Customer.AddressTwo, editedCustomer.AddressTwo);
            Assert.NotEqual(fakeRepo.Customer.Town, editedCustomer.Town);
            Assert.NotEqual(fakeRepo.Customer.State, editedCustomer.State);
            Assert.NotEqual(fakeRepo.Customer.AreaCode, editedCustomer.AreaCode);
            Assert.NotEqual(fakeRepo.Customer.Country, editedCustomer.Country);
            Assert.NotEqual(fakeRepo.Customer.EmailAddress, editedCustomer.EmailAddress);
            Assert.NotEqual(fakeRepo.Customer.TelephoneNumber, editedCustomer.TelephoneNumber);
            Assert.Equal(fakeRepo.Customer.RequestedDeletion, editedCustomer.RequestedDeletion);
            Assert.Equal(fakeRepo.Customer.CanPurchase, editedCustomer.CanPurchase);
            Assert.NotEqual(fakeRepo.Customer.Active, editedCustomer.Active);
        }

        [Fact]
        public async void EditCustomer_CustomerIsInactive_FromOrderingApi_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, setupUser: false, setupApi: true);
            SetMockCustomerRepo(customerActive: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
            SetupUser(controller);
            var editedCustomer = GetEditedDetailsDto();

            //Act
            var result = await controller.Put(customerDto.CustomerId, customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
        }

        [Fact]
        public async void EditCustomer_NullCustomer_FromOrderingApi_ShouldUnprocessableEntity()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true);

            //Act
            var result = await controller.Put(1, null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void EditCustomer_NullCustomer_FromOrderingApi_CheckMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, setupUser: false, setupApi: true);
            SetMockCustomerRepo(authMatch: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
            SetupUser(controller);
            var editedCustomer = GetEditedDetailsDto();

            //Act
            var result = await controller.Put(1, null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.MatchingAuthId(editedCustomer.CustomerId, editedCustomer.CustomerAuthId), Times.Never);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
        }

        [Fact]
        public async void DeleteExistingCustomer_FromOrderingApi_ShouldOk()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true);
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
        }

        [Fact]
        public async void DeleteExistingCustomer_FromOrderingApi_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, setupUser: false, setupApi: true);
            int customerId = 1;

            //Act
            var result = await controller.Delete(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.MatchingAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Once);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void DeleteCustomer_CustomerDoesntExist_FromOrderingApi_ShouldNotFound()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true);
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
        }

        [Fact]
        public async void DeleteExistingCustomer_CustomerDoesntExist_FromOrderingApi_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, setupUser: false, setupApi: true);
            SetMockCustomerRepo(customerExists: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
            SetupUser(controller);
            int customerId = 2;

            //Act
            var result = await controller.Delete(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.MatchingAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void DeleteExistingCustomer_CustomerNotActive_FromOrderingApi_ShouldOk()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true);
            fakeRepo.Customer.Active = false;

            //Act
            var result = await controller.Delete(1);

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
        }

        [Fact]
        public async void DeleteExistingCustomer_CustomerNotActive_FromOrderingApi_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, setupUser: false, setupApi: true);
            SetMockCustomerRepo(customerActive: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
            SetupUser(controller);

            //Act
            var result = await controller.Delete(1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.MatchingAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Once);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async void DeleteExistingCustomer_RepoFails_FromOrderingApi_ShouldNotFound()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true);
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
        }

        [Fact]
        public async void DeleteExistingCustomer_RepoFails_FromOrderingApi_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, setupUser: false, setupApi: true);
            SetMockCustomerRepo(authMatch: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockOrderFacade.Object, mockReviewFacade.Object);
            SetupUser(controller);
            int customerId = 1;

            //Act
            var result = await controller.Delete(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.MatchingAuthId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockOrderFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
        }

    }
}
