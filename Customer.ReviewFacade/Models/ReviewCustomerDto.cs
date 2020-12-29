using System;
using System.Collections.Generic;
using System.Text;

namespace Customer.ReviewFacade.Models
{
    public class ReviewCustomerDto
    {
        public int CustomerId { get; set; }

        public string CustomerAuthId { get; set; }

        public string CustomerName { get; set; }
    }
}
