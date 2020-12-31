using System;
using System.Threading.Tasks;

namespace Customer.AuthFacade
{
    public interface IAuthFacade
    {
        public Task<bool> DeleteAccount(string customerAuthId);
    }
}
