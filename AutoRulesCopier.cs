using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Linq;

namespace AutoRulesCopier
{
    public class AutoRulesCopier
    {
        private string _accessToken;
        private RestClient _restClient;

        public AutoRulesCopier(string apiAddress, string accessToken)
        {
            _accessToken = accessToken;
            _restClient = new RestClient(apiAddress);
        }

        public void Download(string acc)
        {
            var request = new RestRequest($"act_{acc}/adrules_library", Method.GET);
            request.AddQueryParameter("access_token", _accessToken);
            request.AddQueryParameter("fields", "entity_type,evaluation_spec,execution_spec,name,schedule_spec");
            var response = _restClient.Execute(request);
            var json = (JObject)JsonConvert.DeserializeObject(response.Content);
            ErrorChecker.HasErrorsInResponse(json,true);
            if (!string.IsNullOrEmpty(json["error"]?["message"].ToString()))
            {
                Console.WriteLine(
                    $"Ошибка при попытке выполнить запрос:{json["error"]["message"]}");
                return;
            }
            foreach (var rule in json["data"])
            {
                Console.WriteLine($"Найдено правило: {rule["name"]}");
            }
            System.IO.File.WriteAllText("rules.json", response.Content);
            Console.WriteLine("Скачивание правил закончено.");
        }

        public void Upload(string acc)
        {
            if (!System.IO.File.Exists("rules.json"))
            {
                Console.WriteLine("Файл с правилами не существует! Сначала скачайте правила!");
                return;
            }
            
            var jsonTxt = System.IO.File.ReadAllText("rules.json");
            var json = (JObject)JsonConvert.DeserializeObject(jsonTxt);
            ErrorChecker.HasErrorsInResponse(json,true);
            var accSplit = acc.Split(',');
            foreach (var a in accSplit)
            {
				Clear(a);
                foreach (var rule in json["data"])
                {
                    var req = new RestRequest($"act_{a}/adrules_library", Method.POST);
                    req.AddParameter("access_token", _accessToken);
                    req.AddParameter("name", rule["name"]);
                    req.AddParameter("schedule_spec", rule["schedule_spec"]);
                    req.AddParameter("evaluation_spec", rule["evaluation_spec"]);
                    req.AddParameter("execution_spec", rule["execution_spec"]);
                    var resp = _restClient.Execute(req);
                    if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                        Console.WriteLine($"Загрузили правило {rule["name"]} в аккаунт {a}");
                    else
                        Console.WriteLine($"Не смогли загрузить правило {rule["name"]} в аккаунт {a}");
                }
            }
            Console.WriteLine("Загрузка правил закончена.");
        }

        public void Clear(string acc)
        {
            Console.Write($"Вы действительно хотите удалить все автоправила в аккаунте {acc}?(Y/N)");
            var answer= Console.ReadKey();
            Console.WriteLine();
            if (answer.KeyChar!='y'&&answer.KeyChar!='Y') return;
            var request = new RestRequest($"act_{acc}/adrules_library", Method.GET);
            request.AddQueryParameter("access_token", _accessToken);
            request.AddQueryParameter("fields", "name");
            var response = _restClient.Execute(request);
            var json = (JObject)JsonConvert.DeserializeObject(response.Content);
            ErrorChecker.HasErrorsInResponse(json,true);
            if (json["data"]?.Any()??false)
            {
                foreach (var rule in json["data"])
                {
                    Console.WriteLine($"Удаляем правило: {rule["name"]}");
                    request = new RestRequest($"{rule["id"]}", Method.DELETE);
                    request.AddQueryParameter("access_token", _accessToken);
                    var resp = _restClient.Execute(request);
                    if (resp.StatusCode != System.Net.HttpStatusCode.OK)
                        Console.WriteLine("Возникла проблема при удалении этого правила :-(");

                }
            }
            Console.WriteLine("Удаление правил закончено.");
        }
    }
}
