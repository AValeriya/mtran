using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mtran
{
	internal class Parser
	{
		List<string> names;
		List<string> consts;
		List<Token> tokens;

		int currentTokenIndex;

		Token CurrentToken
		{
			get => tokens[currentTokenIndex];
		}

		Token GetCurrentToken
		{
			get => tokens[currentTokenIndex++];
		}

		internal Parser(Lexer lexer)
		{
			names = lexer.Names;
			consts = lexer.Consts;
			tokens = lexer.Tokens;

			currentTokenIndex = 0;
		}

		internal bool Analyse()
		{
			while (!IsEnd())
			{
				if (!IsStatement())
				{
					return false;
				}
			}

			return true;
		}

		bool IsStatement()
		{
			int savedIndex = currentTokenIndex;

			if (IsImport())
			{
				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsEquals())
			{
				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsFunctionCall())
			{
				return true;
			}
			currentTokenIndex = savedIndex;

			return false;
		}

		bool IsImport()
		{
			if (!IsKeyword("from"))
			{
				return false;
			}
			if (!IsName(out string libName))
			{
				return false;
			}
			if (!IsKeyword("import"))
			{
				return false;
			}
			if (!IsName(out string funName))
			{
				return false;
			}

			return true;
		}

		bool IsEquals()
		{
			if (!IsName(out string lhs))
			{
				return false;
			}
			if (!IsSymbol('='))
			{
				return false;
			}
			if (!IsExpression())
			{
				return false;
			}

			return true;
		}

		bool IsFunctionCall()
		{
			if (!IsName(out string fun))
			{
				return false;
			}
			if (!IsSymbol('('))
			{
				return false;
			}
			if (!IsParamList())
			{
				return false;
			}
			if (!IsSymbol(')'))
			{
				return false;
			}

			return true;
		}

		bool IsParamList()
		{
			if (!IsExpression())
			{
				return false;
			}
			while (true)
			{
				if (CurrentToken.symbol == ')')
				{
					break;
				}
				if (!IsSymbol(','))
				{
					return false;
				}
				if (!IsExpression())
				{
					return false;
				}
			}

			return true;
		}

		bool IsExpression()
		{
			int savedIndex = currentTokenIndex;

			if (IsName(out string rhs))
			{
				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsConst())
			{
				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsEmptryArray())
			{
				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsBinaryExpression())
			{
				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsFunctionCall())
			{
				return true;
			}
			currentTokenIndex = savedIndex;

			return false;
		}

		bool IsBinaryExpression()
		{
			if (!IsName(out string lhs))
			{
				return false;
			}
			if (!IsSymbol('.'))
			{
				return false;
			}
			if (!IsName(out string rhs))
			{
				return false;
			}

			return true;
		}

		bool IsEmptryArray()
		{
			if (!IsSymbol('['))
			{
				return false;
			}
			if (!IsSymbol(']'))
			{
				return false;
			}

			return true;
		}

		bool IsName(out string name)
		{
			if (CurrentToken.type != LexemType.NAME)
			{
				name = null;

				return false;
			}

			name = names[GetCurrentToken.nameIndex];

			return true;
		}

		bool IsConst()
		{
			var token = GetCurrentToken;

			if (token.type == LexemType.NUMBER || token.type == LexemType.STRING)
			{
				return true;
			}

			return false;
		}

		bool IsKeyword(string keyword)
		{
			if (!GetCurrentToken.IsKeyword(keyword))
			{
				return false;
			}

			return true;
		}

		bool IsSymbol(char c)
		{
			var token = GetCurrentToken;

			if (token.type == LexemType.SPECIAL && token.symbol == c)
			{
				return true;
			}

			return false;
		}

		bool IsEnd()
		{
			return CurrentToken.type == LexemType.END;
		}
	}
}