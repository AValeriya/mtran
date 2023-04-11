using System;
using System.IO;

namespace mtran
{
	internal class Program
	{
		static void Main()
		{
			Environment.CurrentDirectory += "/files";

			PrintResults("input.txt");

			Console.ReadKey();
		}

		static void PrintResults(string fileName)
		{
			Console.WriteLine($"Analysing {fileName}\n");

			var text = File.ReadAllText(fileName);
			var lexer = new Lexer();
			if (lexer.Analyse(text))
			{
				//lexer.PrintInfo();

				var parser = new Parser(lexer);
				if (parser.Analyse())
				{
					//parser.PrintInfo();

					var semanticAnalyser = new SemanticAnalyser(parser.Ast);
					if (semanticAnalyser.Analyse())
					{
						Console.WriteLine("Great success!");
					}
					else
					{
						Console.Error.WriteLine("Error occured in semantic analyser.");
					}
				}
				else
				{
					Console.Error.WriteLine("Error occured in parser.");
				}
			}
			else
			{
				Console.Error.WriteLine("Error occured in lexer.");
			}

			Console.WriteLine();
		}
	}
}