using System.Net.Http;
using System.Threading.Tasks;

namespace HttpManager
{
    public interface IAccessTokenGetter
    {
        Task<HttpClient> GetToken(HttpClient client, string authUrl, string scopeKey);
    }
}