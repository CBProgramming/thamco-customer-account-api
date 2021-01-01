using Customer.ReviewFacade.Models;
using HttpManager;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Customer.ReviewFacade
{
    public class ReviewCustomerFacade : IReviewCustomerFacade
    {
        private readonly IConfiguration _config;
        private readonly IHttpHandler _handler;

        public ReviewCustomerFacade(IConfiguration config, IHttpHandler handler)
        {
            _config = config;
            _handler = handler;
        }

        public async Task<bool> DeleteCustomer(int customerId)
        {
            HttpClient httpClient = await _handler.GetClient("CustomerAuthServerUrl", "ReviewAPI", "ReviewScope");
            string uri = _config.GetSection("ReviewUri").Value + "/" + customerId;
            if ((await httpClient.DeleteAsync(uri)).IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> EditCustomer(ReviewCustomerDto editedCustomer)
        {
            if (editedCustomer == null)
            {
                return false;
            }
            HttpClient httpClient = await _handler.GetClient("CustomerAuthServerUrl", "ReviewAPI", "ReviewScope");
            string uri = _config.GetSection("ReviewUri").Value;
            if ((await httpClient.PutAsJsonAsync<ReviewCustomerDto>(uri, editedCustomer)).IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> NewCustomer(ReviewCustomerDto newCustomer)
        {
            if (newCustomer == null)
            {
                return false;
            }
            HttpClient httpClient = await _handler.GetClient("CustomerAuthServerUrl", "ReviewAPI", "ReviewScope");
            string uri = _config.GetSection("ReviewUri").Value;
            if ((await httpClient.PostAsJsonAsync<ReviewCustomerDto>(uri, newCustomer)).IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }
    }
}
