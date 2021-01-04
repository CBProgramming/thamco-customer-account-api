using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HttpManager
{
    public class DiscoGetter : IDiscoGetter
    {
        public async Task<string> GetTokenEndPoint(DiscoveryDocumentResponse disco)
        {
            return disco.TokenEndpoint;
        }
    }
}
