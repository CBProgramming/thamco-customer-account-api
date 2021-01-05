using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Customer.AuthFacade
{
    public class FakeAuthFacade : IAuthFacade
    {
        public bool Succeeds = true;
        public string CustomerAuthId;

        public async Task<bool> DeleteAccount(string customerAuthId)
        {
            CustomerAuthId = customerAuthId;
            return Succeeds;
        }
    }
}
