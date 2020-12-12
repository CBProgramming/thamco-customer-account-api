using AutoMapper;
using Customer.AccountAPI;
using Customer.Data;
using Customer.Repository;
using Customer.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Customer.UnitTests
{
    public class CustomerRepoTests
    {
        public CustomerRepoModel customerRepoModel;
        public IMapper mapper;
        public IQueryable<Data.Customer> dbCustomers;
        public Data.Customer dbCustomer;
        public Mock<DbSet<Data.Customer>> mockCustomers;
        public Mock<CustomerDb> mockDbContext;
        public CustomerRepository repo;

        private void SetupCustomerRepoModel()
        {
            customerRepoModel = new CustomerRepoModel
            {
                CustomerId = 2,
                CustomerAuthId = "fakeauthid2",
                GivenName = "Fake2",
                FamilyName = "Name2",
                AddressOne = "Address2 1",
                AddressTwo = "Address2 2",
                Town = "Town2",
                State = "State2",
                AreaCode = "Area Code2",
                Country = "Country2",
                EmailAddress = "email@email.com2",
                TelephoneNumber = "07123456782",
                RequestedDeletion = false,
                CanPurchase = true,
                Active = true
            };
        }

        private void SetupDbCustomer()
        {
            dbCustomer = new Data.Customer
            {
                CustomerId = 1,
                CustomerAuthId = "fakeauthid1",
                GivenName = "Fake1",
                FamilyName = "Name1",
                AddressOne = "Address1 1",
                AddressTwo = "Address1 2",
                Town = "Town1",
                State = "State1",
                AreaCode = "Area Code1",
                Country = "Country1",
                EmailAddress = "email@email.com1",
                TelephoneNumber = "07123456781",
                RequestedDeletion = false,
                CanPurchase = true,
                Active = true
            };
        }

        private void SetupMapper()
        {
            mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            }).CreateMapper();
        }

        private void SetupDbCustomers()
        {
            SetupDbCustomer();
            dbCustomers = new List<Data.Customer>
            {
                dbCustomer
            }.AsQueryable();
        }

        private void SetupMockCustomers()
        {
            mockCustomers = new Mock<DbSet<Data.Customer>>();
            mockCustomers.As<IQueryable<Data.Customer>>().Setup(m => m.Provider).Returns(dbCustomers.Provider);
            mockCustomers.As<IQueryable<Data.Customer>>().Setup(m => m.Expression).Returns(dbCustomers.Expression);
            mockCustomers.As<IQueryable<Data.Customer>>().Setup(m => m.ElementType).Returns(dbCustomers.ElementType);
            mockCustomers.As<IQueryable<Data.Customer>>().Setup(m => m.GetEnumerator()).Returns(dbCustomers.GetEnumerator());
        }

        private void SetupMockDbContext()
        {
            mockDbContext = new Mock<CustomerDb>();
            mockDbContext.Setup(m => m.Customers).Returns(mockCustomers.Object);
            
        }

        private void DefaultSetup()
        {
            SetupMapper();
            SetupCustomerRepoModel();
            SetupDbCustomers();
            SetupMockCustomers();
            SetupMockDbContext();
            repo = new CustomerRepository(mockDbContext.Object, mapper);
        }

        [Fact]
        public async Task NewCustomer_ShouldTrue()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.NewCustomer(customerRepoModel);

            //Assert
            Assert.True(true == result);

            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer.Data.Customer>()), Times.Once());
        }

        [Fact]
        public async Task NewNullCustomer_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.NewCustomer(null);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer.Data.Customer>()), Times.Never()); 
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
            
        }

        [Fact]
        public async Task EditCustomer_ShouldTrue()
        {
            //Arrange
            DefaultSetup();
            customerRepoModel.CustomerId = dbCustomer.CustomerId;
            customerRepoModel.CustomerAuthId = dbCustomer.CustomerAuthId;

            //Act
            var result = await repo.EditCustomer(customerRepoModel);

            //Assert
            Assert.True(true == result);
            Assert.Equal(dbCustomer.CustomerId, customerRepoModel.CustomerId);
            Assert.Equal(dbCustomer.CustomerAuthId, customerRepoModel.CustomerAuthId);
            Assert.Equal(dbCustomer.GivenName, customerRepoModel.GivenName);
            Assert.Equal(dbCustomer.FamilyName, customerRepoModel.FamilyName);
            Assert.Equal(dbCustomer.AddressOne, customerRepoModel.AddressOne);
            Assert.Equal(dbCustomer.AddressTwo, customerRepoModel.AddressTwo);
            Assert.Equal(dbCustomer.Town, customerRepoModel.Town);
            Assert.Equal(dbCustomer.State, customerRepoModel.State);
            Assert.Equal(dbCustomer.AreaCode, customerRepoModel.AreaCode);
            Assert.Equal(dbCustomer.Country, customerRepoModel.Country);
            Assert.Equal(dbCustomer.EmailAddress, customerRepoModel.EmailAddress);
            Assert.Equal(dbCustomer.TelephoneNumber, customerRepoModel.TelephoneNumber);
            Assert.Equal(dbCustomer.RequestedDeletion, customerRepoModel.RequestedDeletion);
            Assert.Equal(dbCustomer.CanPurchase, customerRepoModel.CanPurchase);
            Assert.Equal(dbCustomer.Active, customerRepoModel.Active);
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task EditCustomer_DoesntExist_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.EditCustomer(customerRepoModel);

            //Assert
            Assert.True(false == result);
            Assert.NotEqual(dbCustomer.CustomerId, customerRepoModel.CustomerId);
            Assert.NotEqual(dbCustomer.CustomerAuthId, customerRepoModel.CustomerAuthId);
            Assert.NotEqual(dbCustomer.GivenName, customerRepoModel.GivenName);
            Assert.NotEqual(dbCustomer.FamilyName, customerRepoModel.FamilyName);
            Assert.NotEqual(dbCustomer.AddressOne, customerRepoModel.AddressOne);
            Assert.NotEqual(dbCustomer.AddressTwo, customerRepoModel.AddressTwo);
            Assert.NotEqual(dbCustomer.Town, customerRepoModel.Town);
            Assert.NotEqual(dbCustomer.State, customerRepoModel.State);
            Assert.NotEqual(dbCustomer.AreaCode, customerRepoModel.AreaCode);
            Assert.NotEqual(dbCustomer.Country, customerRepoModel.Country);
            Assert.NotEqual(dbCustomer.EmailAddress, customerRepoModel.EmailAddress);
            Assert.NotEqual(dbCustomer.TelephoneNumber, customerRepoModel.TelephoneNumber);
            Assert.Equal(dbCustomer.RequestedDeletion, customerRepoModel.RequestedDeletion);
            Assert.Equal(dbCustomer.CanPurchase, customerRepoModel.CanPurchase);
            Assert.Equal(dbCustomer.Active, customerRepoModel.Active);
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetExistingCustomer_ShouldOk()
        {
            //Arrange
            DefaultSetup();

            //Act
            var customer = await repo.GetCustomer(1);

            //Assert
            Assert.NotNull(customer);
            Assert.Equal(dbCustomer.CustomerId, customer.CustomerId);
            Assert.Equal(dbCustomer.CustomerAuthId, customer.CustomerAuthId);
            Assert.Equal(dbCustomer.GivenName, customer.GivenName);
            Assert.Equal(dbCustomer.FamilyName, customer.FamilyName);
            Assert.Equal(dbCustomer.AddressOne, customer.AddressOne);
            Assert.Equal(dbCustomer.AddressTwo, customer.AddressTwo);
            Assert.Equal(dbCustomer.Town, customer.Town);
            Assert.Equal(dbCustomer.State, customer.State);
            Assert.Equal(dbCustomer.AreaCode, customer.AreaCode);
            Assert.Equal(dbCustomer.Country, customer.Country);
            Assert.Equal(dbCustomer.EmailAddress, customer.EmailAddress);
            Assert.Equal(dbCustomer.TelephoneNumber, customer.TelephoneNumber);
            Assert.Equal(dbCustomer.RequestedDeletion, customer.RequestedDeletion);
            Assert.Equal(dbCustomer.CanPurchase, customer.CanPurchase);
            Assert.Equal(dbCustomer.Active, customer.Active);
        }

        [Fact]
        public async Task GetCustomer_DoesntExists_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            var customer = await repo.GetCustomer(2);

            //Assert
            Assert.Null(customer);
        }
    }
}
