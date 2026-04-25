using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task_4
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            string publicFile = "public.txt";
            string privateFile = "secret_data.txt";
            File.WriteAllText(publicFile, "Привіт!\nЦе відкритий текст.");
            File.WriteAllText(privateFile, "Паролі та секрети.");

            ISmartTextReader reader = new SmartTextReader();

            reader = new SmartTextChecker(reader);

             reader = new SmartTextReaderLocker(reader, "secret");

            Console.WriteLine("--- Тест 1: Доступний файл ---");
            reader.ReadText(publicFile);

            Console.WriteLine("\n--- Тест 2: Заблокований файл ---");
            reader.ReadText(privateFile);

            File.Delete(publicFile);
            File.Delete(privateFile);

            Console.ReadKey();
        }
    }
}
