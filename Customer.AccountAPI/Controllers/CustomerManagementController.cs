using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        [HttpPut]
        public Task<IActionResult> Get(int? customerId, bool? canPurchase)
        {
            throw new NotImplementedException();
        }

        [HttpDelete]
        public Task<IActionResult> Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}