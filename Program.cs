using Microsoft.Extensions.Configuration;
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
            var arc=new AutoRulesCopier(apiAddress,accessToken);

            switch (args[0])
            {
                case "-d":
                { 
                    arc.Download(args[1]);
                    break;
                }
                case "-u":
                {
                    arc.Upload(args[1]);
                    break;
                }
                case "-x":
                { 
                    arc.Clear(args[1]);
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
