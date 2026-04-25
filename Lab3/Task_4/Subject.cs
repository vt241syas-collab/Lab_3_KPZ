using System;
using System.IO;
using System.Text.RegularExpressions;

public interface ISmartTextReader
{
    char[][] ReadText(string filePath);
}

public class SmartTextReader : ISmartTextReader
{
    public char[][] ReadText(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Файл {filePath} не знайдено.");
            return null;
        }

        string[] lines = File.ReadAllLines(filePath);
        char[][] result = new char[lines.Length][];

        for (int i = 0; i < lines.Length; i++)
        {
            result[i] = lines[i].ToCharArray();
        }

        return result;
    }
}