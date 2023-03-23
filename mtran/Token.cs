namespace mtran
{
	internal class Token
	{
		internal Lexer lexer;
		internal LexemType type;
		internal char symbol;
		internal int nameIndex;
		internal int constIndex;
		internal int keywordIndex;

		internal Token(Lexer lexer, LexemType type, char symbol = '\0', int nameIndex = -1, int constIndex = -1, int keywordIndex = -1)
		{
			this.lexer = lexer;
			this.type = type;
			this.symbol = symbol;
			this.nameIndex = nameIndex;
			this.constIndex = constIndex;
			this.keywordIndex = keywordIndex;
		}

		internal bool IsKeyword(string keyword)
		{
			if (keywordIndex != -1)
			{
				if (Lexer.keywords[keywordIndex] == keyword)
				{
					return true;
				}
			}

			return false;
		}

		public override string ToString()
		{
			if (nameIndex != -1)
			{
				return $"Token (type = {type}, name = \"{lexer.Names[nameIndex]}\")";
			}
			else
			{
				if (constIndex != -1)
				{
					return $"Token (type = {type}, const = \"{lexer.Consts[constIndex]}\")";
				}
				else if (keywordIndex != -1)
				{
					return Lexer.keywords[keywordIndex];
				} else
				{
					return $"Token (type = {type}, symbol = '{symbol}')";
				}
			}
		}
	}
}