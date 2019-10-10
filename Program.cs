using System;
using System.Threading.Tasks;

namespace AutoRulesCopier
{
    class Program
    {
        private const string apiAddress = "https://graph.facebook.com/v4.0/";
        static async Task Main(string[] args)
        {
            Console.WriteLine("Программа для скачивания/загрузки автоправил в Facebook by Даниил Выголов.");
            var re = GetConfiguredRequestExecutor(apiAddress);
            var arc = new AutoRulesCopier(re);
            var navigator = new Navigator(re);

            var operation = Operations.None;
            while (operation != Operations.Exit)
            {
                operation = MainMenu.SelectOperation();

                switch (operation)
                {
                    case Operations.DownloadRules:
                    {
                        var bmId = await navigator.SelectBusinessManagerAsync();
                        var accId = await navigator.SelectAdAccountAsync(bmId, true);
                        await arc.DownloadAsync(accId);
                        break;
                    }
                    case Operations.UploadRules:
                    {
                        var bmId = await navigator.SelectBusinessManagerAsync();
                        var accId = await navigator.SelectAdAccountAsync(bmId, true);
                        await arc.UploadAsync(accId);
                        break;
                    }
                    case Operations.ClearRules:
                    {
                        var bmId = await navigator.SelectBusinessManagerAsync();
                        var accId = await navigator.SelectAdAccountAsync(bmId, true);
                        await arc.ClearAsync(accId);
                        break;
                    }
                }
            }
        }

        private static RequestExecutor GetConfiguredRequestExecutor(string apiAddress)
        {
            Console.Write("Введите access token аккаунта:");
            var ac = Console.ReadLine();
            Console.Write("Введита ip-адрес прокси (Enter,если не нужен):");
            var proxyAddress = Console.ReadLine();
            if (string.IsNullOrEmpty(proxyAddress))
            {
                Console.WriteLine("Не используем прокси!");
                return new RequestExecutor(apiAddress, ac);
            }
            Console.Write("Введите порт прокси:");
            var proxyPort = Console.ReadLine();
            Console.Write("Введите логин прокси:");
            var proxyLogin = Console.ReadLine();
            Console.Write("Введите пароль прокси:");
            var proxyPassword = Console.ReadLine();
            return new RequestExecutor(apiAddress, ac, proxyAddress, proxyPort, proxyLogin, proxyPassword);
        }
    }
}
