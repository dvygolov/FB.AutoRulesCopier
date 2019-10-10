using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Net;
using System.Threading.Tasks;

namespace AutoRulesCopier
{
    public class RequestExecutor
    {
        private readonly string _accessToken;
        public RestClient Rc { get; set; }

        public RequestExecutor(string apiAddress,string accessToken,string proxyAddress=null,string proxyPort=null,string proxyLogin=null, string proxyPassword=null)
        {
            Rc = new RestClient(apiAddress);
            if (!string.IsNullOrEmpty(proxyAddress))
            {
                Rc.Proxy = new WebProxy()
                {
                    Address = new Uri($"http://{proxyAddress}:{proxyPort}"),
                    Credentials = new NetworkCredential()
                    {
                        UserName = proxyLogin,
                        Password = proxyPassword
                    }
                };
            }

            _accessToken = accessToken;
        }


        public async Task<JObject> ExecuteRequestAsync(RestRequest req, bool changeToken = true)
        {
            if (changeToken)
            {
                if (req.Method == Method.GET)
                    req.AddQueryParameter("access_token", _accessToken);
                else
                    req.AddParameter("access_token", _accessToken);
            }
            var resp = await Rc.ExecuteTaskAsync(req);
            var respJson = (JObject)JsonConvert.DeserializeObject(resp.Content);
            return respJson;
        }
    }
}
