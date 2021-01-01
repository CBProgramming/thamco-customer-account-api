using HttpManager;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Customer.AuthFacade
{
    public class AuthFacade : IAuthFacade
    {
        private readonly IHttpHandler _handler;
        private readonly IConfiguration _config;

        public AuthFacade(IConfiguration config, IHttpHandler handler)
        {
            _config = config;
            _handler = handler;
        }
        public async Task<bool> DeleteAccount(string customerAuthId)
        {
            HttpClient httpClient = await _handler.GetClient("CustomerAuthServerUrl", "AuthAPI", "CustomerAuthCustomerScope");
            string uri = _config.GetSection("AuthUri").Value + "/" + customerAuthId;
            if ((await httpClient.DeleteAsync(uri)).IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }
    }
}
