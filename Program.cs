using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;

namespace AutoRulesCopier
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();
            var accessToken = config.GetValue<string>("access_token");
            var apiAddress = config.GetValue<string>("fbapi_address");
            if (string.IsNullOrEmpty(accessToken))
            {
                Console.WriteLine("Не указан access_token!");
                return;
            }
            if (args.Length != 2)
            {
                ShowHelp();
                return;
            }

            var restClient = new RestClient(apiAddress);
            switch (args[0])
            {
                case "-d":
                { 
                    var request = new RestRequest($"act_{args[1]}/adrules_library", Method.GET);
                    request.AddQueryParameter("access_token", accessToken);
                    request.AddQueryParameter("fields", "entity_type,evaluation_spec,execution_spec,name,schedule_spec");
                    var response = restClient.Execute(request);
                    var json = (JObject)JsonConvert.DeserializeObject(response.Content);
                    foreach (var rule in json["data"])
                    {
                        Console.WriteLine($"Найдено правило: {rule["name"]}");
                    }
                    System.IO.File.WriteAllText("rules.json", response.Content);
                    Console.WriteLine("Скачивание правил закончено.");
                    break;
                }
                case "-u":
                {
                    if (!System.IO.File.Exists("rules.json"))
                    {
                        Console.WriteLine("Файл с правилами не существует! Сначала скачайте правила!");
                        return;
                    }
                    var jsonTxt = System.IO.File.ReadAllText("rules.json");
                    var json = (JObject)JsonConvert.DeserializeObject(jsonTxt);
                    var accSplit = args[1].Split(',');
                    foreach (var acc in accSplit)
                    {
                        foreach (var rule in json["data"])
                        {
                            var req = new RestRequest($"act_{acc}/adrules_library", Method.POST);
                            req.AddParameter("access_token", accessToken);
                            req.AddParameter("name", rule["name"]);
                            req.AddParameter("schedule_spec", rule["schedule_spec"]);
                            req.AddParameter("evaluation_spec", rule["evaluation_spec"]);
                            req.AddParameter("execution_spec", rule["execution_spec"]);
                            var resp = restClient.Execute(req);
                            if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                                Console.WriteLine($"Загрузили правило {rule["name"]} в аккаунт {acc}");
                            else
                                Console.WriteLine($"Не смогли загрузить правило {rule["name"]} в аккаунт {acc}");
                        }
                    }
                    Console.WriteLine("Загрузка правил закончена.");
                    break;
                }
                case "-x":
                { 
                    var request = new RestRequest($"act_{args[1]}/adrules_library", Method.GET);
                    request.AddQueryParameter("access_token", accessToken);
                    request.AddQueryParameter("fields", "entity_type,evaluation_spec,execution_spec,name,schedule_spec");
                    var response = restClient.Execute(request);
                    var json = (JObject)JsonConvert.DeserializeObject(response.Content);
                    foreach (var rule in json["data"])
                    {
                        Console.WriteLine($"Удаляем правило: {rule["name"]}");
                        request = new RestRequest($"{rule["id"]}", Method.DELETE);
                        request.AddQueryParameter("access_token",accessToken);
                        var resp=restClient.Execute(request);
                        if (resp.StatusCode!=System.Net.HttpStatusCode.OK)
                            Console.WriteLine("Возникла проблема при удалении этого правила :-(");

                    }
                    Console.WriteLine("Удаление правил закончено.");
                    break;
                }
                default:
                    ShowHelp();
                    break;
            }
        }

        static void ShowHelp()
        {
            Console.WriteLine("Программа для скачивания/загрузки автоправил в Facebook by Даниил Выголов.");
            Console.WriteLine("Варианты запуска:");
            Console.WriteLine("Скачивание настроенных автоправил из рекламного кабинета с указанным ID:");
            Console.WriteLine("-d <ACCOUNT_ID>");
            Console.WriteLine("Загрузка скачанных автоправил в рекламный кабинет с указанным ID:");
            Console.WriteLine("-u <ACCOUNT_ID>");
            Console.WriteLine("Примечание: при загрузке можно указать несколько ID кабинетов через запятую БЕЗ ПРОБЕЛА.");
            Console.WriteLine("Удаление всех автоправил в рекламном кабинете с указанным ID:");
            Console.WriteLine("-x <ACCOUNT_ID>");
        }
    }
}
