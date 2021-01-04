using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HttpManager
{
    public interface IDiscoGetter
    {
        Task<string> GetTokenEndPoint(DiscoveryDocumentResponse disco);
    }
}
