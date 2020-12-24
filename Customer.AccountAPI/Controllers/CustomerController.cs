using AutoMapper;
using Customer.AccountAPI.Models;
using Customer.OrderFacade;
using Customer.OrderFacade.Models;
using Customer.Repository;
using Customer.Repository.Models;
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
    [Authorize(Policy = "CustomerOnly")]
    public class CustomerController : ControllerBase
    {
        private readonly ILogger<CustomerController> _logger;
        private readonly ICustomerRepository _customerRepository;
        private readonly IMapper _mapper;
        private readonly IOrderFacade _facade;
        private string authId, clientId;

        public CustomerController(ILogger<CustomerController> logger, ICustomerRepository customerRepository, IMapper mapper, IOrderFacade facade)
        {
            _logger = logger;
            _customerRepository = customerRepository;
            _mapper = mapper;
            _facade = facade;
        }

        private void getTokenDetails()
        {
            authId = User
                .Claims
                .FirstOrDefault(c => c.Type == "sub")?.Value;
            clientId = User
                .Claims
                .FirstOrDefault(c => c.Type == "client_id")?.Value;
        }

        // GET: api/<controller>
        [HttpGet("{customerId}")]
        //[Authorize]
        public async Task<IActionResult> Get([FromRoute] int customerId)
        {
            if (await _customerRepository.CustomerExists(customerId)
                && await _customerRepository.IsCustomerActive(customerId))
            {
                var customer = _mapper.Map<CustomerDto>(await _customerRepository.GetCustomer(customerId));
                if (customer != null)
                {
                    if (User != null && User.Claims != null)
                    {
                        getTokenDetails();
                        if ((authId != null && customer.CustomerAuthId == authId)
                            || (clientId != null && clientId.Equals("customer_ordering_api")))
                        {
                            return Ok(customer);
                        }
                    }
                    return Forbid();
                }
            }
            return NotFound();
        }

        // POST api/<controller>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CustomerDto customer)
        {
            if(customer != null)
            {
                customer.Active = true;
                return await NewOrEditedCustomer(customer);
            }
            return UnprocessableEntity();
        }

        // PUT api/<controller>/5
        [HttpPut("{customerId}")]
        public async Task<IActionResult> Put([FromRoute] int customerId, [FromBody] CustomerDto customer)
        {
            if (customer != null)
            {
                customer.CustomerId = customerId;
                customer.Active = true;
                return await NewOrEditedCustomer(customer);
            }
            return UnprocessableEntity();
        }

        private async Task<IActionResult> NewOrEditedCustomer(CustomerDto customer)
        {
            if (customer != null)
            {
                if (!await _customerRepository.CustomerExists(customer.CustomerId))
                {
                    getTokenDetails();
                    if (clientId != null && (clientId.Equals("customer_ordering_api") 
                        || clientId.Equals("customer_web_app")))
                    {
                        customer.CustomerId = await _customerRepository.NewCustomer(_mapper.Map<CustomerRepoModel>(customer));
                        if (customer.CustomerId != 0)
                        {
                            if (clientId != "customer_ordering_api")
                            {
                                if (!await _facade.NewCustomer(_mapper.Map<OrderingCustomerDto>(customer)))
                                {
                                    //write to local db to be reattempted later
                                }
                            }
                            return Ok();
                        }
                    }

                }
                else
                {
                    if (await _customerRepository.IsCustomerActive(customer.CustomerId))
                    {
                        if (User != null && User.Claims != null)
                        {
                            getTokenDetails();
                            if ((authId != null && customer.CustomerAuthId == authId)
                                || (clientId != null && clientId.Equals("customer_ordering_api")))
                            {
                                if (await _customerRepository.EditCustomer(_mapper.Map<CustomerRepoModel>(customer)))
                                {
                                    if (clientId != "customer_ordering_api")
                                    {
                                        if (!await _facade.EditCustomer(_mapper.Map<OrderingCustomerDto>(customer)))
                                        {
                                            //write to local db to be reattempted later
                                        }
                                    }
                                    return Ok();
                                }
                            }
                        }
                        return Forbid();
                    }
                }
                return NotFound();
            }
            return UnprocessableEntity();
        }

        // DELETE api/<controller>/5
        [HttpDelete("{customerId}")]
        public async Task<IActionResult> Delete([FromRoute] int customerId)
        {
            if (User != null && User.Claims != null)
            {
                getTokenDetails();
                if ((authId != null && await _customerRepository.MatchingAuthId(customerId, authId))
                    || (clientId != null && clientId.Equals("customer_ordering_api")))
                {
                    if (await _customerRepository.CustomerExists(customerId)
                           && await AnonymiseCustomer(customerId))
                    {
                        if (clientId != "customer_ordering_api")
                        {
                            if (!await _facade.DeleteCustomer(customerId))
                            {
                                //write to local db to be reattempted later
                            }
                        }
                        return Ok();
                    }
                    return NotFound();
                }
            }  
            return Forbid();
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