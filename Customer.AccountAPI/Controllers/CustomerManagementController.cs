﻿using AutoMapper;
using Customer.AccountAPI.Models;
using Customer.AuthFacade;
using Customer.OrderFacade;
using Customer.OrderFacade.Models;
using Customer.Repository;
using Customer.Repository.Models;
using Customer.ReviewFacade;
using Customer.ReviewFacade.Models;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Policy = "StaffOnly")]
    public class CustomerManagementController : ControllerBase
    {
        private readonly ILogger<CustomerManagementController> _logger;
        private readonly ICustomerRepository _customerRepository;
        private readonly IMapper _mapper;
        private readonly IOrderFacade _orderFacade;
        private readonly IReviewCustomerFacade _reviewFacade;
        private readonly IAuthFacade _authFacade;

        public CustomerManagementController(ILogger<CustomerManagementController> logger, ICustomerRepository customerRepository, 
            IMapper mapper, IOrderFacade orderFacade, IReviewCustomerFacade reviewFacade, IAuthFacade authFacade)
        {
            _logger = logger;
            _customerRepository = customerRepository;
            _mapper = mapper;
            _orderFacade = orderFacade;
            _reviewFacade = reviewFacade;
            _authFacade = authFacade;
        }

        [HttpPut("{customerId}")]
        public async Task<IActionResult> Put([FromRoute]int customerId, [FromQuery] bool canPurchase)
        {
            var customer = _mapper.Map<CustomerDto>(await _customerRepository.GetCustomer(customerId));
            if (customer == null)
            {
                return NotFound();
            }
            if (canPurchase)
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
                if (!await _orderFacade.EditCustomer(_mapper.Map<OrderingCustomerDto>(customer)))
                {
                    //write to local db top be sent later
                }
                var editedCustomer = await _customerRepository.GetCustomer(customer.CustomerId);
                var reviewCustomer = new ReviewCustomerDto
                {
                    CustomerId = editedCustomer.CustomerId,
                    CustomerAuthId = editedCustomer.CustomerAuthId,
                    CustomerName = editedCustomer.GivenName + " " + editedCustomer.FamilyName
                };
                if (!await _reviewFacade.EditCustomer(reviewCustomer))
                {
                    //write to local db to be reattempted later
                }
                return Ok();
            }
            return NotFound();
        }

        [HttpDelete("{customerId}")]
        public async Task<IActionResult> Delete([FromRoute] int customerId)
        {
            var customer = await _customerRepository.GetCustomer(customerId);
            if (customer != null
                    && await AnonymiseCustomer(customerId))
            {
                if (!await _orderFacade.DeleteCustomer(customerId))
                {
                    //write to local db to be reattempted later
                }
                if (!await _reviewFacade.DeleteCustomer(customerId))
                {
                    //write to local db to be reattempted later
                }
                if (!await _authFacade.DeleteAccount(customer.CustomerAuthId))
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