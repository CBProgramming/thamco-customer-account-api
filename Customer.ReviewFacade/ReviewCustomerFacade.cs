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
        private readonly IHttpHandler _handler;
        private string customerAuthUrl;
        private string customerReviewApi;
        private string customerReviewScope;
        private string reviewUri;

        public ReviewCustomerFacade(IConfiguration config, IHttpHandler handler)
        {
            _handler = handler;
            if (config != null)
            {
                customerAuthUrl = config.GetSection("CustomerAuthServerUrlKey").Value;
                customerReviewApi = config.GetSection("ReviewAPIKey").Value;
                customerReviewScope = config.GetSection("ReviewScopeKey").Value;
                reviewUri = config.GetSection("ReviewUri").Value;
            }
        }

        public async Task<bool> DeleteCustomer(int customerId)
        {
            if (!ValidConfigStrings())
            {
                return false;
            }
            HttpClient httpClient = await _handler.GetClient(customerAuthUrl, customerReviewApi, customerReviewScope);
            if (httpClient == null)
            {
                return false;
            }
            reviewUri = reviewUri + customerId;
            if ((await httpClient.DeleteAsync(reviewUri)).IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> EditCustomer(ReviewCustomerDto editedCustomer)
        {
            if (editedCustomer == null
                || !ValidConfigStrings())
            {
                return false;
            }
            HttpClient httpClient = await _handler.GetClient(customerAuthUrl, customerReviewApi, customerReviewScope);
            if (httpClient == null)
            {
                return false;
            }
            if ((await httpClient.PutAsJsonAsync<ReviewCustomerDto>(reviewUri, editedCustomer)).IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> NewCustomer(ReviewCustomerDto newCustomer)
        {
            if (newCustomer == null
                || !ValidConfigStrings())
            {
                return false;
            }
            HttpClient httpClient = await _handler.GetClient(customerAuthUrl, customerReviewApi, customerReviewScope);
            if (httpClient == null)
            {
                return false;
            }
            if ((await httpClient.PostAsJsonAsync<ReviewCustomerDto>(reviewUri, newCustomer)).IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        private bool ValidConfigStrings()
        {
            return !string.IsNullOrEmpty(customerAuthUrl)
                    && !string.IsNullOrEmpty(customerReviewApi)
                    && !string.IsNullOrEmpty(customerReviewScope)
                    && !string.IsNullOrEmpty(reviewUri);
        }
    }
}
