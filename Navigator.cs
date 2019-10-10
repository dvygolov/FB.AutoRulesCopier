using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoRulesCopier
{
    public class Navigator
    {
        private readonly RequestExecutor _re;
        public Navigator(RequestExecutor re)
        {
            _re = re;
        }

        public async Task<string> SelectBusinessManagerAsync()
        {
            var bms = await GetAllBmsAsync();

            for (int i = 0; i < bms.Count; i++)
            {
                var bm = bms[i];
                Console.WriteLine($"{i + 1}. {bm["name"]}");
            }

            bool goodRes;
            int index;
            do
            {
                Console.Write("Выберите БМ, введя его номер, и нажмите Enter:");
                var readIndex = Console.ReadLine();
                goodRes = int.TryParse(readIndex, out index);
                if (index > bms.Count) goodRes = false;
            }
            while (!goodRes);
            return bms[index-1]["id"].ToString();
        }

        public async Task<string> SelectAdAccountAsync(string bmid, bool includeBanned = false)
        {
            var accounts = await GetBmsAdAccountsAsync(bmid, includeBanned);

            for (int i = 0; i < accounts.Count; i++)
            {
                var acc = accounts[i];
                Console.WriteLine($"{i + 1}. {acc["name"]}");
            }

            int index;
            bool goodRes;
            do
            {
                Console.Write("Выберите РК, введя его номер, и нажмите Enter:");
                var readIndex = Console.ReadLine();
                goodRes = int.TryParse(readIndex, out index);
                if (index > accounts.Count) goodRes = false;
            }
            while (!goodRes);
            return accounts[index-1]["id"].ToString().TrimStart(new[] { 'a', 'c', 't', '_' });
        }


        private async Task<List<JToken>> GetAllBmsAsync()
        {
            var request = new RestRequest($"me/businesses", Method.GET);
            var json= await _re.ExecuteRequestAsync(request);
            var bms = json["data"].ToList();
            return bms;
        }

        public async Task<List<JToken>> GetBmsAdAccountsAsync(string bmid, bool includeBanned = false)
        {
            var request = new RestRequest($"{bmid}/client_ad_accounts", Method.GET);
            request.AddQueryParameter("fields", "name,account_status");
            var json=await _re.ExecuteRequestAsync(request);
            var accounts = json["data"].ToList();
            request = new RestRequest($"{bmid}/owned_ad_accounts", Method.GET);
            request.AddQueryParameter("fields", "name,account_status");
            json=await _re.ExecuteRequestAsync(request);
            accounts.AddRange(json["data"].ToList());
            //Исключаем забаненные
            if (!includeBanned)
                accounts = accounts.Where(acc => acc["account_status"].ToString() != "2").ToList();
            return accounts;
        }
    }
}