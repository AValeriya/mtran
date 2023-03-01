using System;
using System.IO;

namespace MyCoolLexer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Environment.CurrentDirectory += "/files";

            PrintResults("input.txt");
            //PrintResults("sort.txt");

            Console.ReadKey();
        }

        static void PrintResults(string fileName)
        {
            Console.WriteLine($"Analysing {fileName}\n");

            var text = File.ReadAllText(fileName);
            var lexer = new Lexer(text);

            Console.WriteLine("\nNames:\n");
            lexer.Names.ForEach(n => Console.WriteLine(n));
            Console.WriteLine("\nConsts:\n");
            lexer.Consts.ForEach(c => Console.WriteLine(c));
            Console.WriteLine("\nTokens:\n");
            lexer.Tokens.ForEach(t => Console.WriteLine(t));

            Console.WriteLine();
        }
    }
}