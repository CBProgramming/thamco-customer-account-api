using AutoMapper;
using Customer.AccountAPI.Models;
using Customer.OrderFacade;
using Customer.OrderFacade.Models;
using Customer.Repository;
using Customer.Repository.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Customer.AccountAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerManagementController : ControllerBase
    {
        private readonly ILogger<CustomerManagementController> _logger;
        private readonly ICustomerRepository _customerRepository;
        private readonly IMapper _mapper;
        private readonly IOrderFacade _facade;

        public CustomerManagementController(ILogger<CustomerManagementController> logger, ICustomerRepository customerRepository, 
            IMapper mapper, IOrderFacade facade)
        {
            _logger = logger;
            _customerRepository = customerRepository;
            _mapper = mapper;
            _facade = facade;
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromRoute]int customerId, [FromBody] bool canPurchase)
        {
            var customer = _mapper.Map<CustomerDto>(await _customerRepository.GetCustomer(customerId));
            if (customer == null)
            {
                return NotFound();
            }
            if(canPurchase)
            {
                if (customer.Active)
                {
                    return await EditCustomer(customer, canPurchase);
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return await EditCustomer(customer, canPurchase);
            }
        }

        private async Task<IActionResult> EditCustomer(CustomerDto customer, bool canPurchase)
        {
            customer.CanPurchase = canPurchase;
            if (await _customerRepository.EditCustomer(_mapper.Map<CustomerRepoModel>(customer)))
            {
                if (!await _facade.EditCustomer(_mapper.Map<OrderingCustomerDto>(customer)))
                {
                    //write to local db top be sent later
                }
                return Ok();
            }
            return NotFound();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromRoute] int customerId)
        {
            if (await _customerRepository.CustomerExists(customerId)
                    && await AnonymiseCustomer(customerId))
            {
                if (!await _facade.DeleteCustomer(customerId))
                {
                    //write to local db to be reattempted later
                }
                return Ok();
            }
            return NotFound();
        }

        private async Task<bool> AnonymiseCustomer(int customerId)
        {
            var customer = new CustomerDto
            {
                CustomerId = customerId,
                GivenName = "Anonymised",
                FamilyName = "Anonymised",
                AddressOne = "Anonymised",
                AddressTwo = "Anonymised",
                Town = "Anonymised",
                State = "Anonymised",
                AreaCode = "Anonymised",
                Country = "Anonymised",
                EmailAddress = "anon@anon.com",
                TelephoneNumber = "00000000000",
                CanPurchase = false,
                Active = false
            };
            return await _customerRepository.AnonymiseCustomer(_mapper.Map<CustomerRepoModel>(customer));
        }
    }
}