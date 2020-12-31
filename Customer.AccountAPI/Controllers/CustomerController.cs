using AutoMapper;
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
    [Authorize(Policy = "CustomerOnly")]
    public class CustomerController : ControllerBase
    {
        private readonly ILogger<CustomerController> _logger;
        private readonly ICustomerRepository _customerRepository;
        private readonly IMapper _mapper;
        private readonly IOrderFacade _orderFacade;
        private readonly IReviewCustomerFacade _reviewFacade;
        private readonly IAuthFacade _authFacade;
        private string authId, clientId, tokenCustomerId;

        public CustomerController(ILogger<CustomerController> logger, ICustomerRepository customerRepository, IMapper mapper, 
            IOrderFacade orderFacade, IReviewCustomerFacade reviewFacade, IAuthFacade authFacade)
        {
            _logger = logger;
            _customerRepository = customerRepository;
            _mapper = mapper;
            _orderFacade = orderFacade;
            _reviewFacade = reviewFacade;
            _authFacade = authFacade;
        }

        private void getTokenDetails()
        {
            if (User!= null && User.Claims != null)
            {
                authId = User
                .Claims
                .FirstOrDefault(c => c.Type == "sub")?.Value;
                clientId = User
                    .Claims
                    .FirstOrDefault(c => c.Type == "client_id")?.Value;
                tokenCustomerId = User
                    .Claims
                    .FirstOrDefault(c => c.Type == "id")?.Value;
            }
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
                getTokenDetails();
                if (clientId != null && clientId.Equals("customer_web_app"))
                {
                    int customerId = 0;
                    int.TryParse(tokenCustomerId, out customerId);
                    if (customerId < 1 )
                    {
                        return NotFound();
                    }
                    if ((customer.CustomerId != 0 && customerId != customer.CustomerId)
                        || authId != customer.CustomerAuthId)
                        {
                        return Forbid();
                    }
                    customer.CustomerId = customerId;
                }
                if (!await _customerRepository.CustomerExists(customer.CustomerId))
                {
                    if (await _customerRepository.NewCustomer(_mapper.Map<CustomerRepoModel>(customer)))
                    {
                        if (!await _orderFacade.NewCustomer(_mapper.Map<OrderingCustomerDto>(customer)))
                        {
                            //write to local db to be reattempted later
                        }
                        var reviewCustomer = new ReviewCustomerDto
                        {
                            CustomerId = customer.CustomerId,
                            CustomerAuthId = authId,
                            CustomerName = customer.GivenName + " " + customer.FamilyName
                        };
                        if (!await _reviewFacade.NewCustomer(reviewCustomer))
                        {
                            //write to local db to be reattempted later
                        }
                    }
                    return Ok();
                }
                else
                {
                    if (await _customerRepository.IsCustomerActive(customer.CustomerId))
                    {
/*                        if (User != null && User.Claims != null)
                        {
                            return Forbid();
                        }*/
                        if ((authId != null && customer.CustomerAuthId == authId)
                            || (clientId != null && clientId.Equals("customer_ordering_api")))
                        {
                            if (await _customerRepository.EditCustomer(_mapper.Map<CustomerRepoModel>(customer)))
                            {
                                if (clientId != "customer_ordering_api")
                                {
                                    if (!await _orderFacade.EditCustomer(_mapper.Map<OrderingCustomerDto>(customer)))
                                    {
                                        //write to local db to be reattempted later
                                    }
                                }
                                var reviewCustomer = new ReviewCustomerDto
                                {
                                    CustomerId = customer.CustomerId,
                                    CustomerAuthId = authId,
                                    CustomerName = customer.GivenName + " " + customer.FamilyName
                                };
                                if (!await _reviewFacade.EditCustomer(reviewCustomer))
                                {
                                    //write to local db to be reattempted later
                                }
                                return Ok();
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
                    var customer = _mapper.Map<CustomerDto>(await _customerRepository.GetCustomer(customerId));
                    if (customer != null
                           && await AnonymiseCustomer(customerId))
                    {
                        if (clientId != "customer_ordering_api")
                        {
                            if (!await _orderFacade.DeleteCustomer(customerId))
                            {
                                //write to local db to be reattempted later
                            }
                        }
                        if (!await _reviewFacade.DeleteCustomer(customerId))
                        {
                            //write to local db to be reattempted later
                        }
                        if (! await _authFacade.DeleteAccount(customer.CustomerAuthId))
                        {
                            //write to local db to be reattempted later
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