using System;

namespace AutoRulesCopier
{
    public static class MainMenu
    {
        public static Operations SelectOperation()
        {
            int operation=0;
            bool operationSelected = false;
            while (!operationSelected)
            {
                Console.WriteLine("Выберите операцию:");
                Console.WriteLine("1.Скачать правила");
                Console.WriteLine("2.Залить правила");
                Console.WriteLine("3.Очистить правила");
                Console.WriteLine("4.Выход");
                Console.Write("Введите номер:");
                var opKey = Console.ReadKey();
                Console.WriteLine();
                if (!int.TryParse(opKey.KeyChar.ToString(), out operation)) continue;
                operationSelected = true;
            }
            return (Operations) operation;
        }

    }
}
