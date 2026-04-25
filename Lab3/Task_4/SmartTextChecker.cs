using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task_4
{
    public class SmartTextChecker : ISmartTextReader
    {
        private readonly ISmartTextReader _reader;

        public SmartTextChecker(ISmartTextReader reader)
        {
            _reader = reader;
        }

        public char[][] ReadText(string filePath)
        {
            Console.WriteLine($"[LOG]: Спроба відкриття файлу: {filePath}");

            char[][] data = _reader.ReadText(filePath);

            if (data != null)
            {
                Console.WriteLine($"[LOG]: Файл успішно прочитано.");

                int totalChars = 0;
                foreach (var row in data) totalChars += row.Length;

                Console.WriteLine($"[STAT]: Кількість рядків: {data.Length}");
                Console.WriteLine($"[STAT]: Загальна кількість символів: {totalChars}");
                Console.WriteLine($"[LOG]: Файл закрито.");
            }

            return data;
        }
    }
}
