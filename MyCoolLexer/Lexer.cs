using System;
using System.Collections.Generic;
using System.Globalization;

namespace MyCoolLexer
{
	internal class Lexer
	{
		const string specialSymbols = "(){}[]<>,.:;!@%|&^*-+=/?";
		readonly static List<string> keywords = new List<string>() { "if", "else", "while", "for", "import", "from" };

		List<string> names;
		List<string> consts;
		List<Token> tokens;

		LexemType currentTokenType;

		internal List<string> Names => names;
		internal List<string> Consts => consts;
		internal List<Token> Tokens => tokens;

		internal Lexer()
		{
			names = new List<string>();
			consts = new List<string>();
			tokens = new List<Token>();
		}

		internal bool Analyse(string text)
		{
			string temp = "";
			char stringOpening = ' ';
			char currentSymbol;

			currentTokenType = LexemType.NONE;

			bool CheckForString()
			{
				var type = GetLexemType(currentSymbol);
				if (type == LexemType.STRING)
				{
					if (stringOpening != currentSymbol)
					{
						ReportError($"String quotes are inconsistent: {stringOpening} and {currentSymbol}");

						return false;
					}
					if (!AddConst(LexemType.STRING, temp))
					{
						return false;
					}
					temp = "";
					currentTokenType = LexemType.NONE;
				}
				else
				{
					temp += currentSymbol;
				}

				return true;
			}

			bool SwitchToken(LexemType type)
			{
				switch (currentTokenType)
				{
					case LexemType.NAME:
						AddName(temp);
						temp = "";
						break;
					case LexemType.NUMBER:
						if (!AddConst(LexemType.NUMBER, temp))
						{
							return false;
						}
						temp = "";
						break;
					case LexemType.ERROR:
						return false;
					default:
						break;
				}
				currentTokenType = type;

				return true;
			}

			void ProcessLexem(LexemType type)
			{
				switch (type)
				{
					case LexemType.NAME:
					case LexemType.NUMBER:
						temp += currentSymbol;
						break;
					case LexemType.SPECIAL:
						tokens.Add(new Token(this, LexemType.SPECIAL, currentSymbol));
						break;
					case LexemType.STRING:
						currentTokenType = LexemType.STRING;
						stringOpening = currentSymbol;
						temp = "";
						break;
					default:
						break;
				}
			}

			bool ProcessRemainingLexem()
			{
				switch (currentTokenType)
				{
					case LexemType.NAME:
						AddName(temp);
						break;
					case LexemType.NUMBER:
						if (!AddConst(LexemType.NUMBER, temp))
						{
							return false;
						}
						break;
					case LexemType.STRING:
						ReportError($"String did not end!:{temp}");

						return false;
					default:
						break;
				}

				return true;
			}

			for (int i = 0; i < text.Length; i++)
			{
				currentSymbol = text[i];
				if (currentTokenType == LexemType.STRING)
				{
					if (!CheckForString())
					{
						return false;
					}
				}
				else
				{
					if (currentSymbol == '#')
					{
						while (i < text.Length)
						{
							currentSymbol = text[i++];
							if (currentSymbol == '\n' || currentSymbol == '\r')
							{
								break;
							}
						}
					}
					else
					{
						var lexemType = GetLexemType(currentSymbol);
						if (lexemType != currentTokenType)
						{
							if (!SwitchToken(lexemType))
							{
								return false;
							}
						}

						ProcessLexem(lexemType);
					}
				}
			}

			if (!ProcessRemainingLexem())
			{
				return false;
			}

			tokens.Add(new Token(this, LexemType.END));

			return true;
		}

		void AddName(string name)
		{
			int index = names.IndexOf(name);
			if (index == -1)
			{
				names.Add(name);
				tokens.Add(new Token(this, LexemType.NAME, nameIndex: names.Count - 1));
			}
			else
			{
				tokens.Add(new Token(this, LexemType.NAME, nameIndex: index));
			}
		}

		bool AddConst(LexemType type, string value)
		{
			if (type == LexemType.NUMBER)
			{
				if (!double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double d))
				{
					if (!long.TryParse(value, out long l))
					{
						ReportError($"Invalid numeric value: {value}");

						return false;
					}
				}
			}
			int index = consts.IndexOf(value);
			if (index == -1)
			{
				consts.Add(value);
				tokens.Add(new Token(this, type, constIndex: consts.Count - 1));
			}
			else
			{
				tokens.Add(new Token(this, type, constIndex: index));
			}

			return true;
		}

		void ReportError(string error)
		{
			Console.Error.WriteLine($"Error: {error}");
		}

		LexemType GetLexemType(char c)
		{
			if (c == '\"' || c == '\'')
			{
				return LexemType.STRING;
			}
			else if (char.IsLetter(c) || c == '_')
			{
				return LexemType.NAME;
			}
			else if (char.IsDigit(c) || (currentTokenType == LexemType.NUMBER && c == '.'))
			{
				return LexemType.NUMBER;
			}
			else if (char.IsWhiteSpace(c))
			{
				return LexemType.SPACE;
			}
			else if (specialSymbols.Contains(c.ToString()))
			{
				return LexemType.SPECIAL;
			}
			else
			{
				ReportError($"Invalid symbol used: {c}");

				return LexemType.ERROR;
			}
		}

		internal void PrintInfo()
		{
			PrintTableLine(false);
			Console.Write("|");
			Console.Write(AddSpaces("Names:", 36));
			Console.WriteLine(AddSpaces("|", 36));
			PrintTableLine();

			foreach (var name in names)
			{
				int maxLen = 16;
				Console.Write('|');
				Console.Write(AddSpaces(name, maxLen - name.Length));
				Console.Write('|');
				if (keywords.Contains(name))
				{
					string desc = "Keyword";
					Console.Write(AddSpaces(desc, 61 - desc.Length));
				}
				else
				{
					string desc = "Name";
					Console.Write(AddSpaces(desc, 61 - desc.Length));
				}
				Console.WriteLine('|');
			}

			PrintTableLine();
			Console.WriteLine();

			PrintTableLine(false);
			Console.Write("|");
			Console.Write(AddSpaces("Consts:", 36));
			Console.WriteLine(AddSpaces("|", 35));
			PrintTableLine();
			for (int i = 0; i < consts.Count; i++)
			{
				var cnst = consts[i];
				var token = tokens.Find(t => t.constIndex == i);
				int maxLen = 16;
				Console.Write('|');
				Console.Write(AddSpaces(cnst, maxLen - cnst.Length));
				Console.Write('|');
				if (token.type == LexemType.NUMBER)
				{
					string desc = "Numeric constant";
					Console.Write(AddSpaces(desc, 61 - desc.Length));
				}
				else
				{
					string desc = "String constant";
					Console.Write(AddSpaces(desc, 61 - desc.Length));
				}
				Console.WriteLine('|');
			}

			PrintTableLine();
			Console.WriteLine();

			PrintTableLine(false);
			Console.Write("|");
			Console.Write(AddSpaces("Operations:", 34));
			Console.WriteLine(AddSpaces("|", 33));
			PrintTableLine();
			for (int i = 0; i < tokens.Count; i++)
			{
				var token = tokens[i];
				if (token.symbol != '\0')
				{
					int maxLen = 16;
					Console.Write('|');
					Console.Write(AddSpaces("" + token.symbol, maxLen - 1));
					Console.Write('|');
					string desc = $"{token.symbol} operation";
					Console.Write(AddSpaces(desc, 61 - desc.Length));
					Console.WriteLine('|');
				}
			}

			PrintTableLine();
			Console.WriteLine();
		}

		void PrintTableLine(bool withDelimeter = true)
		{
			if (withDelimeter)
			{
				Console.WriteLine("+================+=============================================================+");
			}
			else
			{
				Console.WriteLine("+==============================================================================+");
			}
		}

		string AddSpaces(string text, int count)
		{
			string s = "";

			for (int i = 0; i < count; i++)
			{
				s += ' ';
			}
			s += text;

			return s;
		}
	}
}
