using AspNet.Security.OpenIdConnect.Primitives;
using AutoMapper;
using Customer.AccountAPI;
using Customer.AccountAPI.Controllers;
using Customer.AccountAPI.Models;
using Customer.OrderFacade;
using Customer.OrderFacade.Models;
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

        private void SetMockCustomerRepo (bool customerExists = true, bool customerActive = true, bool succeeds = true, bool authMatch = true)
        {
            mockRepo = new Mock<ICustomerRepository>(MockBehavior.Strict);
            mockRepo.Setup(repo => repo.CustomerExists(It.IsAny<int>())).ReturnsAsync(customerExists).Verifiable();
            mockRepo.Setup(repo => repo.IsCustomerActive(It.IsAny<int>())).ReturnsAsync(customerActive).Verifiable();
            mockRepo.Setup(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>())).ReturnsAsync(succeeds).Verifiable();
            mockRepo.Setup(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>())).ReturnsAsync(succeeds).Verifiable();
            mockRepo.Setup(repo => repo.MatchingAuthId(It.IsAny<int>(),It.IsAny<string>())).ReturnsAsync(authMatch).Verifiable();
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

        private void SetMockOrderFacade (bool customerExists = true, bool succeeds = true)
        {
            mockFacade = new Mock<IOrderFacade>(MockBehavior.Strict);
            mockFacade.Setup(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>())).ReturnsAsync(succeeds).Verifiable();
            mockFacade.Setup(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>())).ReturnsAsync(succeeds).Verifiable();
            mockFacade.Setup(facade => facade.DeleteCustomer(It.IsAny<int>())).ReturnsAsync(succeeds).Verifiable();
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

        private void SetFakeFacade()
        {
            fakeFacade = new FakeOrderFacade();
        }

        private void DefaultSetup(bool withMocks = false)
        {
            SetStandardCustomerDto();
            SetStandardCustomerRepoModel();
            SetFakeRepo(customerRepoModel);
            SetFakeFacade();
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
            SetMockCustomerRepo();
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

        [Fact]
        public async void GetCustomer_RepoFailure_VerifyMocks()
        {
            //Arrange
            DefaultSetup();
            SetMockCustomerRepo(succeeds: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockFacade.Object);
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
            Assert.Equal(customerRepoModel.CustomerId, customerDto.CustomerId);
            Assert.Equal(customerRepoModel.CustomerAuthId, customerDto.CustomerAuthId);
            Assert.Equal(customerRepoModel.GivenName, customerDto.GivenName);
            Assert.Equal(customerRepoModel.FamilyName, customerDto.FamilyName);
            Assert.Equal(customerRepoModel.AddressOne, customerDto.AddressOne);
            Assert.Equal(customerRepoModel.AddressTwo, customerDto.AddressTwo);
            Assert.Equal(customerRepoModel.Town, customerDto.Town);
            Assert.Equal(customerRepoModel.State, customerDto.State);
            Assert.Equal(customerRepoModel.AreaCode, customerDto.AreaCode);
            Assert.Equal(customerRepoModel.Country, customerDto.Country);
            Assert.Equal(customerRepoModel.EmailAddress, customerDto.EmailAddress);
            Assert.Equal(customerRepoModel.TelephoneNumber, customerDto.TelephoneNumber);
            Assert.Equal(customerRepoModel.RequestedDeletion, customerDto.RequestedDeletion);
            Assert.Equal(customerRepoModel.CanPurchase, customerDto.CanPurchase);
            Assert.Equal(customerRepoModel.Active, customerDto.Active);
        }

        [Fact]
        public async void PostNewCustomer_VerifyMocks()
        {
            //Arrange
            DefaultSetup(true);
            SetMockCustomerRepo(customerExists: false, customerActive: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockFacade.Object);
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
            mockFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Once);
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
            Assert.Equal(customerRepoModel.CustomerId, customerDto.CustomerId);
            Assert.Equal(customerRepoModel.CustomerAuthId, customerDto.CustomerAuthId);
            Assert.Equal(customerRepoModel.GivenName, customerDto.GivenName);
            Assert.Equal(customerRepoModel.FamilyName, customerDto.FamilyName);
            Assert.Equal(customerRepoModel.AddressOne, customerDto.AddressOne);
            Assert.Equal(customerRepoModel.AddressTwo, customerDto.AddressTwo);
            Assert.Equal(customerRepoModel.Town, customerDto.Town);
            Assert.Equal(customerRepoModel.State, customerDto.State);
            Assert.Equal(customerRepoModel.AreaCode, customerDto.AreaCode);
            Assert.Equal(customerRepoModel.Country, customerDto.Country);
            Assert.Equal(customerRepoModel.EmailAddress, customerDto.EmailAddress);
            Assert.Equal(customerRepoModel.TelephoneNumber, customerDto.TelephoneNumber);
            Assert.Equal(customerRepoModel.RequestedDeletion, customerDto.RequestedDeletion);
            Assert.Equal(customerRepoModel.CanPurchase, customerDto.CanPurchase);
            Assert.Equal(customerRepoModel.Active, customerDto.Active);
        }

        [Fact]
        public async void PostNewCustomer_CustomerAlreadyExists_VerifyMocks()
        {
            //Arrange
            DefaultSetup(true);
            SetMockCustomerRepo();
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockFacade.Object);
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
            mockFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Once);
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
            Assert.Equal(customerRepoModel.CustomerId, editedCustomer.CustomerId);
            Assert.Equal(customerRepoModel.CustomerAuthId, editedCustomer.CustomerAuthId);
            Assert.NotEqual(customerRepoModel.GivenName, editedCustomer.GivenName);
            Assert.NotEqual(customerRepoModel.FamilyName, editedCustomer.FamilyName);
            Assert.NotEqual(customerRepoModel.AddressOne, editedCustomer.AddressOne);
            Assert.NotEqual(customerRepoModel.AddressTwo, editedCustomer.AddressTwo);
            Assert.NotEqual(customerRepoModel.Town, editedCustomer.Town);
            Assert.NotEqual(customerRepoModel.State, editedCustomer.State);
            Assert.NotEqual(customerRepoModel.AreaCode, editedCustomer.AreaCode);
            Assert.NotEqual(customerRepoModel.Country, editedCustomer.Country);
            Assert.NotEqual(customerRepoModel.EmailAddress, editedCustomer.EmailAddress);
            Assert.NotEqual(customerRepoModel.TelephoneNumber, editedCustomer.TelephoneNumber);
            Assert.Equal(customerRepoModel.RequestedDeletion, editedCustomer.RequestedDeletion);
            Assert.Equal(customerRepoModel.CanPurchase, editedCustomer.CanPurchase);
            Assert.NotEqual(customerRepoModel.Active, editedCustomer.Active);
        }

        [Fact]
        public async void PostNewCustomer_CustomerAlreadyExistsAndInactive_VerifyMocks()
        {
            //Arrange
            DefaultSetup(true);
            SetMockCustomerRepo(customerActive: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockFacade.Object);
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
            mockFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
        }

        [Fact]
        public async void PostNewCustomer_AuthIdDoesntMatch_ShouldNotForbid()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer.CustomerAuthId = "realId";
            var editedCustomer = GetEditedDetailsDto();

            //Act
            var result = await controller.Post(editedCustomer);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Customer);
            Assert.Equal(customerRepoModel.CustomerId, editedCustomer.CustomerId);
            Assert.NotEqual(customerRepoModel.CustomerAuthId, editedCustomer.CustomerAuthId);
            Assert.NotEqual(customerRepoModel.GivenName, editedCustomer.GivenName);
            Assert.NotEqual(customerRepoModel.FamilyName, editedCustomer.FamilyName);
            Assert.NotEqual(customerRepoModel.AddressOne, editedCustomer.AddressOne);
            Assert.NotEqual(customerRepoModel.AddressTwo, editedCustomer.AddressTwo);
            Assert.NotEqual(customerRepoModel.Town, editedCustomer.Town);
            Assert.NotEqual(customerRepoModel.State, editedCustomer.State);
            Assert.NotEqual(customerRepoModel.AreaCode, editedCustomer.AreaCode);
            Assert.NotEqual(customerRepoModel.Country, editedCustomer.Country);
            Assert.NotEqual(customerRepoModel.EmailAddress, editedCustomer.EmailAddress);
            Assert.NotEqual(customerRepoModel.TelephoneNumber, editedCustomer.TelephoneNumber);
            Assert.Equal(customerRepoModel.RequestedDeletion, editedCustomer.RequestedDeletion);
            Assert.Equal(customerRepoModel.CanPurchase, editedCustomer.CanPurchase);
            Assert.Equal(customerRepoModel.Active, editedCustomer.Active);
        }

        [Fact]
        public async void PostNewCustomer_AuthIdDoesntMatch_CheckMocks()
        {
            //Arrange
            DefaultSetup();
            SetMockCustomerRepo(authMatch: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockFacade.Object);
            SetupUser(controller);
            var editedCustomer = GetEditedDetailsDto();

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
            mockRepo.Verify(repo => repo.MatchingAuthId(editedCustomer.CustomerId,editedCustomer.CustomerAuthId), Times.Once);
            mockFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
        }

        [Fact]
        public async void EditCustomer_CustomerDoesntExist_ShouldOkCreatingNewCustomer()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer = null;

            //Act
            var result = await controller.Put(customerDto.CustomerId,customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Customer);
            Assert.Equal(customerRepoModel.CustomerId, customerDto.CustomerId);
            Assert.Equal(customerRepoModel.CustomerAuthId, customerDto.CustomerAuthId);
            Assert.Equal(customerRepoModel.GivenName, customerDto.GivenName);
            Assert.Equal(customerRepoModel.FamilyName, customerDto.FamilyName);
            Assert.Equal(customerRepoModel.AddressOne, customerDto.AddressOne);
            Assert.Equal(customerRepoModel.AddressTwo, customerDto.AddressTwo);
            Assert.Equal(customerRepoModel.Town, customerDto.Town);
            Assert.Equal(customerRepoModel.State, customerDto.State);
            Assert.Equal(customerRepoModel.AreaCode, customerDto.AreaCode);
            Assert.Equal(customerRepoModel.Country, customerDto.Country);
            Assert.Equal(customerRepoModel.EmailAddress, customerDto.EmailAddress);
            Assert.Equal(customerRepoModel.TelephoneNumber, customerDto.TelephoneNumber);
            Assert.Equal(customerRepoModel.RequestedDeletion, customerDto.RequestedDeletion);
            Assert.Equal(customerRepoModel.CanPurchase, customerDto.CanPurchase);
            Assert.Equal(customerRepoModel.Active, customerDto.Active);
        }

        [Fact]
        public async void EditCustomer_CustomerDoesntExist_VerifyMocks()
        {
            //Arrange
            DefaultSetup(true);
            SetMockCustomerRepo(customerExists: false, customerActive: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockFacade.Object);
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
            mockFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Once);
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
            Assert.Equal(customerRepoModel.CustomerId, customerDto.CustomerId);
            Assert.Equal(customerRepoModel.CustomerAuthId, customerDto.CustomerAuthId);
            Assert.Equal(customerRepoModel.GivenName, customerDto.GivenName);
            Assert.Equal(customerRepoModel.FamilyName, customerDto.FamilyName);
            Assert.Equal(customerRepoModel.AddressOne, customerDto.AddressOne);
            Assert.Equal(customerRepoModel.AddressTwo, customerDto.AddressTwo);
            Assert.Equal(customerRepoModel.Town, customerDto.Town);
            Assert.Equal(customerRepoModel.State, customerDto.State);
            Assert.Equal(customerRepoModel.AreaCode, customerDto.AreaCode);
            Assert.Equal(customerRepoModel.Country, customerDto.Country);
            Assert.Equal(customerRepoModel.EmailAddress, customerDto.EmailAddress);
            Assert.Equal(customerRepoModel.TelephoneNumber, customerDto.TelephoneNumber);
            Assert.Equal(customerRepoModel.RequestedDeletion, customerDto.RequestedDeletion);
            Assert.Equal(customerRepoModel.CanPurchase, customerDto.CanPurchase);
            Assert.Equal(customerRepoModel.Active, customerDto.Active);
        }

        [Fact]
        public async void EditCustomer_VerifyMocks()
        {
            //Arrange
            DefaultSetup(true);
            SetMockCustomerRepo();
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockFacade.Object);
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
            mockFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Once);
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
            Assert.Equal(customerRepoModel.CustomerId, editedCustomer.CustomerId);
            Assert.Equal(customerRepoModel.CustomerAuthId, editedCustomer.CustomerAuthId);
            Assert.NotEqual(customerRepoModel.GivenName, editedCustomer.GivenName);
            Assert.NotEqual(customerRepoModel.FamilyName, editedCustomer.FamilyName);
            Assert.NotEqual(customerRepoModel.AddressOne, editedCustomer.AddressOne);
            Assert.NotEqual(customerRepoModel.AddressTwo, editedCustomer.AddressTwo);
            Assert.NotEqual(customerRepoModel.Town, editedCustomer.Town);
            Assert.NotEqual(customerRepoModel.State, editedCustomer.State);
            Assert.NotEqual(customerRepoModel.AreaCode, editedCustomer.AreaCode);
            Assert.NotEqual(customerRepoModel.Country, editedCustomer.Country);
            Assert.NotEqual(customerRepoModel.EmailAddress, editedCustomer.EmailAddress);
            Assert.NotEqual(customerRepoModel.TelephoneNumber, editedCustomer.TelephoneNumber);
            Assert.Equal(customerRepoModel.RequestedDeletion, editedCustomer.RequestedDeletion);
            Assert.Equal(customerRepoModel.CanPurchase, editedCustomer.CanPurchase);
            Assert.NotEqual(customerRepoModel.Active, editedCustomer.Active);
        }

        [Fact]
        public async void EditCustomer_CustomerIsInactive_VerifyMocks()
        {
            //Arrange
            DefaultSetup(true);
            SetMockCustomerRepo(customerActive: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockFacade.Object);
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
            mockFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
        }

        [Fact]
        public async void EditCustomer_AuthIdDoesntMatch_ShouldNotForbid()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer.CustomerAuthId = "realId";
            var editedCustomer = GetEditedDetailsDto();

            //Act
            var result = await controller.Put(customerDto.CustomerId, customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Customer);
            Assert.Equal(customerRepoModel.CustomerId, editedCustomer.CustomerId);
            Assert.NotEqual(customerRepoModel.CustomerAuthId, editedCustomer.CustomerAuthId);
            Assert.NotEqual(customerRepoModel.GivenName, editedCustomer.GivenName);
            Assert.NotEqual(customerRepoModel.FamilyName, editedCustomer.FamilyName);
            Assert.NotEqual(customerRepoModel.AddressOne, editedCustomer.AddressOne);
            Assert.NotEqual(customerRepoModel.AddressTwo, editedCustomer.AddressTwo);
            Assert.NotEqual(customerRepoModel.Town, editedCustomer.Town);
            Assert.NotEqual(customerRepoModel.State, editedCustomer.State);
            Assert.NotEqual(customerRepoModel.AreaCode, editedCustomer.AreaCode);
            Assert.NotEqual(customerRepoModel.Country, editedCustomer.Country);
            Assert.NotEqual(customerRepoModel.EmailAddress, editedCustomer.EmailAddress);
            Assert.NotEqual(customerRepoModel.TelephoneNumber, editedCustomer.TelephoneNumber);
            Assert.Equal(customerRepoModel.RequestedDeletion, editedCustomer.RequestedDeletion);
            Assert.Equal(customerRepoModel.CanPurchase, editedCustomer.CanPurchase);
            Assert.Equal(customerRepoModel.Active, editedCustomer.Active);
        }

        [Fact]
        public async void EditCustomer_AuthIdDoesntMatch_CheckMocks()
        {
            //Arrange
            DefaultSetup();
            SetMockCustomerRepo(authMatch: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockFacade.Object);
            SetupUser(controller);
            var editedCustomer = GetEditedDetailsDto();

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
            mockRepo.Verify(repo => repo.MatchingAuthId(editedCustomer.CustomerId, editedCustomer.CustomerAuthId), Times.Once);
            mockFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
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
            DefaultSetup(true);
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
            mockFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Once);
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
            DefaultSetup(true);
            SetMockCustomerRepo(customerExists: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockFacade.Object);
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
            mockFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
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
            DefaultSetup();
            SetMockCustomerRepo(customerActive: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockFacade.Object);
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
            mockFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Once);
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
            DefaultSetup();
            SetMockCustomerRepo(authMatch: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockFacade.Object);
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
            mockFacade.Verify(facade => facade.NewCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockFacade.Verify(facade => facade.EditCustomer(It.IsAny<OrderingCustomerDto>()), Times.Never);
            mockFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
        }
    }
}
