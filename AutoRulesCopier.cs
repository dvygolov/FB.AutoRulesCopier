using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Threading.Tasks;

namespace AutoRulesCopier
{
    public class AutoRulesCopier
    {
        private readonly RequestExecutor _re;

        public AutoRulesCopier(RequestExecutor re)
        {
            _re = re;
        }

        public async Task DownloadAsync(string acc)
        {
            var request = new RestRequest($"act_{acc}/adrules_library", Method.GET);
            request.AddQueryParameter("fields", "entity_type,evaluation_spec,execution_spec,name,schedule_spec");
            var json= await _re.ExecuteRequestAsync(request);
            ErrorChecker.HasErrorsInResponse(json,true);
            foreach (var rule in json["data"])
            {
                Console.WriteLine($"Найдено правило: {rule["name"]}");
            }
            Console.Write("Введите имя файла для сохранения правил:");
            var fileName=Console.ReadLine();
            System.IO.File.WriteAllText($"{fileName}.rls", json.ToString());
            Console.WriteLine("Скачивание правил закончено.");
        }

        public async Task UploadAsync(string acc)
        {
            Console.Write("Введите имя файла из которого будем грузить правила:");
            var fileName=Console.ReadLine();
            if (!System.IO.File.Exists($"{fileName}.rls"))
            {
                Console.WriteLine("Файл с правилами не существует! Сначала скачайте правила!");
                return;
            }
            Console.WriteLine("При загрузке новых автоправил, вероятно, надо почистить старые?");
            await ClearAsync(acc);

            var jsonTxt = System.IO.File.ReadAllText($"{fileName}.rls");
            var json = (JObject)JsonConvert.DeserializeObject(jsonTxt);
            var accSplit = acc.Split(',');
            foreach (var a in accSplit)
            {
                foreach (var rule in json["data"])
                {
                    var req = new RestRequest($"act_{a}/adrules_library", Method.POST);
                    req.AddParameter("name", rule["name"]);
                    req.AddParameter("schedule_spec", rule["schedule_spec"]);
                    req.AddParameter("evaluation_spec", rule["evaluation_spec"]);
                    req.AddParameter("execution_spec", rule["execution_spec"]);
                    var js=await _re.ExecuteRequestAsync(req);
                    if (!ErrorChecker.HasErrorsInResponse(js))
                        Console.WriteLine($"Загрузили правило {rule["name"]} в аккаунт {a}");
                    else
                        Console.WriteLine($"Не смогли загрузить правило {rule["name"]} в аккаунт {a}");
                }
            }
            Console.WriteLine("Загрузка правил закончена.");
        }

        public async Task ClearAsync(string acc)
        {
            Console.Write($"Вы хотите удалить все автоправила в аккаунте {acc}?(Y/N)");
            var answer = Console.ReadKey();
            Console.WriteLine();
            if (answer.KeyChar != 'y' && answer.KeyChar != 'Y') return;
            var request = new RestRequest($"act_{acc}/adrules_library", Method.GET);
            request.AddQueryParameter("fields", "entity_type,evaluation_spec,execution_spec,name,schedule_spec");
            var json=await _re.ExecuteRequestAsync(request);
            foreach (var rule in json["data"])
            {
                Console.WriteLine($"Удаляем правило: {rule["name"]}");
                request = new RestRequest($"{rule["id"]}", Method.DELETE);
                var js=await _re.ExecuteRequestAsync(request);
                if (ErrorChecker.HasErrorsInResponse(js))
                    Console.WriteLine("Возникла проблема при удалении этого правила :-(");

            }
            Console.WriteLine("Удаление правил закончено.");
        }
    }
}
