using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main
{
    public class FileLoggerAdapter : Logger
    {
        private readonly FileWriter _fileWriter;

        public FileLoggerAdapter(FileWriter fileWriter)
        {
            _fileWriter = fileWriter;
        }

        public override void Log(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            _fileWriter.WriteLine($"[File-Log]: {message}");
            Console.ResetColor();
        }

        public override void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            _fileWriter.WriteLine($"[File-Error]: {message}");
            Console.ResetColor();
        }

        public override void Warn(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            _fileWriter.WriteLine($"[File-Warn]: {message}");
            Console.ResetColor();
        }
    }
}
