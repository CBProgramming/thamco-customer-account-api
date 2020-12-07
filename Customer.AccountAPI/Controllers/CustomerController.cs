using Customer.AccountAPI.Models;
using Customer.Repository;
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
    public class CustomerController : ControllerBase
    {
        private readonly ILogger<CustomerController> _logger;
        private readonly ICustomerRepository _orderRepository;
        private readonly IMapper _mapper;

        public CustomerController(ILogger<CustomerController> logger, IOrderRepository orderRepository, IMapper mapper)
        {
            _logger = logger;
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{customerId}")]
        [Authorize]
        public async Task<IActionResult> Get([FromRoute] int customerId)
        {
            //reaqd from access token
            var userId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            if (await _orderRepository.CustomerExists(customerId))
            {
                var customer = _mapper.Map<CustomerDto>(await _orderRepository.GetCustomer(customerId));
                if (customer != null)
                {
                    return Ok(customer);
                }
            }
            /*            var customer = _mapper.Map<CustomerDto>(await _orderRepository.GetCustomer(customerId));
                        if (customer != null)
                        {
                            return Ok(customer);
                        }*/
            return NotFound();

            //return Ok();
        }

        // POST api/<controller>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CustomerDto customer)
        {
            if (await _orderRepository.NewCustomer(_mapper.Map<CustomerEFModel>(customer)))
            {
                return Ok();
            }
            return NotFound();
            //return await NewOrEditedCustomer(customer);
        }

        // PUT api/<controller>/5
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] CustomerDto customer)
        {
            return await NewOrEditedCustomer(customer);
        }

        private async Task<IActionResult> NewOrEditedCustomer(CustomerDto customer)
        {
            if (!await _orderRepository.CustomerExists(customer.CustomerId))
            {
                if (await _orderRepository.NewCustomer(_mapper.Map<CustomerEFModel>(customer)))
                {
                    return Ok();
                }
            }
            else
            {
                if (await _orderRepository.EditCustomer(_mapper.Map<CustomerEFModel>(customer)))
                {
                    return Ok();
                }
            }
            return NotFound();
        }

        // DELETE api/<controller>/5
        [HttpDelete("{customerId}")]
        public async Task<IActionResult> Delete([FromRoute] int customerId)
        {
            if (await AnonymiseCustomer(customerId))
            {
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
            return await _orderRepository.AnonymiseCustomer(_mapper.Map<CustomerEFModel>(customer));
        }
    }
}