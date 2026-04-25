using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            
            Logger standardLogger = new Logger();
            Console.WriteLine("--- Робота стандартного Logger ---");
            standardLogger.Log("Все працює нормально.");
            standardLogger.Warn("Це попередження!");
            standardLogger.Error("Сталася критична помилка!");

            Console.WriteLine();

            
            FileWriter writer = new FileWriter();
            Logger fileLogger = new FileLoggerAdapter(writer);

            Console.WriteLine("--- Робота FileLoggerAdapter (через FileWriter) ---");
            fileLogger.Log("Запис логу через адаптер файлу.");
            fileLogger.Warn("Попередження записано в 'файл'.");
            fileLogger.Error("Помилка записана в 'файл'.");

            Console.ReadKey();
        }
    }
}
