using AutoMapper;
using Customer.AccountAPI;
using Customer.AccountAPI.Controllers;
using Customer.AccountAPI.Models;
using Customer.Repository;
using Customer.Repository.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Xunit;

namespace Customer.UnitTests
{
    public class AccountAPITests
    {
        private CustomerDto GetStandardCustomerDto(int? id)
        {
            return new CustomerDto
            {
                CustomerId = id ?? 1,
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

        private CustomerRepoModel GetStandardCustomerRepoModel(int? id)
        {
            return new CustomerRepoModel
            {
                CustomerId = id ?? 1,
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

        private FakeCustomerRepository GetFakeRepo(CustomerRepoModel customer)
        {
            return new FakeCustomerRepository
            {
                Customer = customer
            };
        }

        private IMapper GetMapper()
        {
            return new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            }).CreateMapper();
        }

        private ILogger<CustomerController> GetLogger()
        {
            return new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider()
                .GetService<ILoggerFactory>()
                .CreateLogger<CustomerController>();
        }

        [Fact]
        public void Test1()
        {
            var customer = GetStandardCustomerDto(1);
        }
    }
}
