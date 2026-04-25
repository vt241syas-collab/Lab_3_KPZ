using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Task_4
{
    public class SmartTextReaderLocker : ISmartTextReader
    {
        private readonly ISmartTextReader _reader;
        private readonly Regex _lockPattern;

        public SmartTextReaderLocker(ISmartTextReader reader, string pattern)
        {
            _reader = reader;
            _lockPattern = new Regex(pattern, RegexOptions.IgnoreCase);
        }

        public char[][] ReadText(string filePath)
        {
            if (_lockPattern.IsMatch(filePath))
            {
                Console.WriteLine($"[SECURITY]: Access denied to file: {filePath}!");
                return null;
            }

            return _reader.ReadText(filePath);
        }
    }
}
