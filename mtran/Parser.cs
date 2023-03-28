using System.Collections.Generic;

namespace mtran
{
	internal class Ast
	{
		internal List<Statement> statements;
	}

	internal class Statement { }

	internal class Import : Statement
	{
		internal string module;
		internal string name;
	}

	internal class Assignment : Statement
	{
		internal Expression left;
		internal Expression right;
		internal AssignmentType type;
	}

	internal class If : Statement
	{
		internal Expression condition;
	}

	internal class Else : Statement { }

	internal class For : Statement
	{
		internal Expression variable;
		internal Expression range;
	}

	internal class While : Statement
	{
		internal Expression condition;
	}

	internal class Expression : Statement
	{
		internal Expressiontype type;
		internal Expression left;
		internal Expression right;
		internal string value;
	}

	internal class FunctionCall : Expression
	{
		internal List<Expression> parameters;
	}

	internal enum AssignmentType
	{
		assignment,
		add,
		sub,
		mul,
		div,
	}

	internal enum Expressiontype
	{
		none,
		name,
		number,
		str,
		range,
		arr,
		func,
		sub,
		add,
		mul,
		div,
		dot,
		index,
		less,
		lessOrEquals,
		greater,
		greaterOrEquals,
		equals,
		or,
		and,
		xor,
		not,
	}

	internal class Parser
	{
		readonly List<string> names;
		readonly List<string> consts;
		readonly List<Token> tokens;

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

		internal bool Analyse(out Ast ast)
		{
			ast = null;

			List<Statement> statements = new List<Statement>();

			while (!IsEnd())
			{
				if (!IsStatement(out Statement statement))
				{
					return false;
				}
				statements.Add(statement);
			}

			ast = new Ast()
			{
				statements = statements
			};

			return true;
		}

		bool IsStatement(out Statement statement)
		{
			int savedIndex = currentTokenIndex;
			statement = null;

			if (IsImport(out Import import))
			{
				statement = import;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsAssignmentStatement(out Assignment ass))
			{
				statement = ass;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsCompoundStatement(out Assignment compoundAss))
			{
				statement = compoundAss;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsFunctionCall(out FunctionCall funcCall))
			{
				statement = funcCall;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsIfStatement(out If ifStatement))
			{
				statement = ifStatement;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsElseStatement(out Else elseStatement))
			{
				statement = elseStatement;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsElifStatement(out If elifStatement))
			{
				statement = elifStatement;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsForStatement(out For forStatement))
			{
				statement = forStatement;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsWhileStatement(out While whileStatement))
			{
				statement = whileStatement;

				return true;
			}
			currentTokenIndex = savedIndex;

			return false;
		}

		bool IsImport(out Import import)
		{
			import = null;

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

			import = new Import()
			{
				module = libName,
				name = funName
			};

			return true;
		}

		bool IsAssignmentStatement(out Assignment ass)
		{
			ass = null;

			if (!IsExpression(out Expression left))
			{
				return false;
			}
			if (!IsSymbol('='))
			{
				return false;
			}
			if (!IsExpression(out Expression right))
			{
				return false;
			}

			ass = new Assignment()
			{
				type = AssignmentType.assignment,
				left = left,
				right = right,
			};

			return true;
		}

		bool IsCompoundStatement(out Assignment ass)
		{
			ass = null;

			if (!IsDotOrIndexOrNameOrConst(out Expression left))
			{
				return false;
			}
			if (!IsCompoundOperation(out AssignmentType type))
			{
				return false;
			}
			if (!IsSymbol('='))
			{
				return false;
			}
			if (!IsExpression(out Expression right))
			{
				return false;
			}

			ass = new Assignment()
			{
				type = type,
				left = left,
				right = right
			};

			return true;
		}
		
		bool IsCompoundOperation(out AssignmentType type)
		{
			int savedIndex = currentTokenIndex;
			type = AssignmentType.assignment;

			if (IsSymbol('-'))
			{
				type = AssignmentType.sub;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsSymbol('+'))
			{
				type = AssignmentType.add;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsSymbol('/'))
			{
				type = AssignmentType.div;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsSymbol('*'))
			{
				type = AssignmentType.mul;

				return true;
			}
			currentTokenIndex = savedIndex;

			return false;
		}

		bool IsFunctionCall(out FunctionCall e)
		{
			e = null;

			if (!IsFunction(out Expression func))
			{
				return false;
			}
			if (!IsSymbol('('))
			{
				return false;
			}
			if (!IsParamList(out List<Expression> list))
			{
				return false;
			}
			if (!IsSymbol(')'))
			{
				return false;
			}

			e = new FunctionCall()
			{
				type = Expressiontype.func,
				left = func,
				parameters = list
			};

			return true;
		}

		bool IsFunction(out Expression e)
		{
			int savedIndex = currentTokenIndex;
			e = null;

			if (IsMethod(out Expression meth))
			{
				e = meth;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsName(out string fun))
			{
				e = new Expression()
				{
					type = Expressiontype.name,
					value = fun
				};

				return true;
			}
			currentTokenIndex = savedIndex;

			return false;
		}

		bool IsMethod(out Expression e)
		{
			e = null;

			if (!IsName(out string obj))
			{
				return false;
			}
			if (!IsSymbol('.'))
			{
				return false;
			}
			if (!IsName(out string method))
			{
				return false;
			}

			e = new Expression()
			{
				type = Expressiontype.dot,
				left = new Expression()
				{
					type = Expressiontype.name,
					value = obj
				},
				right = new Expression()
				{
					type = Expressiontype.name,
					value = method
				}
			};

			return true;
		}

		bool IsIfStatement(out If ifStatement)
		{
			ifStatement = null;

			if (!IsKeyword("if"))
			{
				return false;
			}
			if (!IsExpression(out Expression condition))
			{
				return false;
			}
			if (!IsSymbol(':'))
			{
				return false;
			}

			ifStatement = new If()
			{
				condition = condition
			};

			return true;
		}

		bool IsElifStatement(out If ifStatement)
		{
			ifStatement = null;

			if (!IsKeyword("elif"))
			{
				return false;
			}
			if (!IsExpression(out Expression condition))
			{
				return false;
			}
			if (!IsSymbol(':'))
			{
				return false;
			}

			ifStatement = new If()
			{
				condition = condition
			};

			return true;
		}

		bool IsElseStatement(out Else elseStatement)
		{
			elseStatement = null;

			if (!IsKeyword("else"))
			{
				return false;
			}
			if (!IsSymbol(':'))
			{
				return false;
			}

			elseStatement = new Else();

			return true;
		}

		bool IsForStatement(out For forStatement)
		{
			forStatement = null;

			if (!IsKeyword("for"))
			{
				return false;
			}
			if (!IsExpression(out Expression variable))
			{
				return false;
			}
			if (!IsKeyword("in"))
			{
				return false;
			}
			if (!IsExpression(out Expression range))
			{
				return false;
			}
			if (!IsSymbol(':'))
			{
				return false;
			}

			forStatement = new For()
			{
				variable = variable,
				range = range
			};

			return true;
		}

		bool IsWhileStatement(out While whileStatement)
		{
			whileStatement = null;

			if (!IsKeyword("while"))
			{
				return false;
			}
			if (!IsExpression(out Expression condition))
			{
				return false;
			}
			if (!IsSymbol(':'))
			{
				return false;
			}

			whileStatement = new While()
			{
				condition = condition
			};

			return true;
		}

		bool IsParamList(out List<Expression> list)
		{
			list = null;

			if (!IsExpression(out Expression first))
			{
				return false;
			}
			list = new List<Expression>() { first };
			while (true)
			{
				if (CurrentToken.symbol == ')')
				{
					break;
				}
				if (!IsSymbol(','))
				{
					list = null;

					return false;
				}
				if (!IsExpression(out Expression following))
				{
					list = null;

					return false;
				}
				list.Add(following);
			}

			return true;
		}

		bool IsExpression(out Expression e)
		{
			int savedIndex = currentTokenIndex;
			e = null;

			if (IsExpressionInBraces(out Expression braces))
			{
				e = braces;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsFunctionCall(out FunctionCall funcCall))
			{
				e = funcCall;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsBinaryExpression(out Expression binary))
			{
				e = binary;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsRange(out Expression range))
			{
				e = range;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsDotOrIndexOrNameOrConst(out Expression dotOrIndexOrConst))
			{
				e = dotOrIndexOrConst;

				return true;
			}
			currentTokenIndex = savedIndex;

			return false;
		}

		bool IsExpressionInBraces(out Expression e)
		{
			e = null;

			if (!IsSymbol('('))
			{
				return false;
			}
			if (!IsExpression(out Expression temp))
			{
				return false;
			}
			if (!IsSymbol(')'))
			{
				return false;
			}

			e = temp;

			return true;
		}

		bool IsBinaryExpression(out Expression binary)
		{
			binary = null;

			if (!IsDotOrIndexOrNameOrConst(out Expression left))
			{
				return false;
			}
			if (!IsBinaryOperation(out Expressiontype type))
			{
				return false;
			}
			if (!IsExpression(out Expression right))
			{
				return false;
			}

			binary = new Expression()
			{
				left = left,
				type = type,
				right = right
			};

			return true;
		}

		bool IsBinaryOperation(out Expressiontype type)
		{
			int savedIndex = currentTokenIndex;
			type = Expressiontype.none;

			if (IsSymbol('-'))
			{
				type = Expressiontype.sub;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsSymbol('+'))
			{
				type = Expressiontype.add;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsSymbol('/'))
			{
				type = Expressiontype.div;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsSymbol('*'))
			{
				type = Expressiontype.mul;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsSymbol('<'))
			{
				type = Expressiontype.less;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsSymbol('>'))
			{
				type = Expressiontype.greater;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsSymbol('|'))
			{
				type = Expressiontype.or;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsSymbol('&'))
			{
				type = Expressiontype.and;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsSymbol('^'))
			{
				type = Expressiontype.xor;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsLessOrEquals())
			{
				type = Expressiontype.lessOrEquals;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsGreaterOrEquals())
			{
				type = Expressiontype.greaterOrEquals;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsEquals())
			{
				type = Expressiontype.equals;

				return true;
			}
			currentTokenIndex = savedIndex;

			return false;
		}

		bool IsLessOrEquals()
		{
			if (!IsSymbol('<'))
			{
				return false;
			}
			if (!IsSymbol('='))
			{
				return false;
			}

			return true;
		}

		bool IsGreaterOrEquals()
		{
			if (!IsSymbol('>'))
			{
				return false;
			}
			if (!IsSymbol('='))
			{
				return false;
			}

			return true;
		}

		bool IsEquals()
		{
			if (!IsSymbol('='))
			{
				return false;
			}
			if (!IsSymbol('='))
			{
				return false;
			}

			return true;
		}

		bool IsIndexExpression(out Expression e)
		{
			e = null;

			if (!IsName(out string left))
			{
				return false;
			}
			if (!IsSymbol('['))
			{
				return false;
			}
			if (!IsExpression(out Expression right))
			{
				return false;
			}
			if (!IsSymbol(']'))
			{
				return false;
			}

			e = new Expression()
			{
				type = Expressiontype.index,
				left = new Expression()
				{
					type = Expressiontype.name,
					value = left
				},
				right = right
			};

			return true;
		}

		bool IsNameOrIndex(out Expression e)
		{
			int savedIndex = currentTokenIndex;
			e = null;

			if (IsIndexExpression(out Expression index))
			{
				e = index;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsName(out string name))
			{
				e = new Expression()
				{
					type = Expressiontype.name,
					value = name
				};

				return true;
			}
			currentTokenIndex = savedIndex;

			return false;
		}

		bool IsDotExpression(out Expression e)
		{
			e = null;

			if (!IsNameOrIndex(out Expression left))
			{
				return false;
			}
			if (!IsSymbol('.'))
			{
				return false;
			}
			if (!IsDotOrIndexOrNameOrConst(out Expression right))
			{
				return false;
			}

			e = new Expression()
			{
				type = Expressiontype.dot,
				left = left,
				right = right
			};

			return true;
		}

		bool IsDotOrIndexOrNameOrConst(out Expression e)
		{
			int savedIndex = currentTokenIndex;
			e = null;

			if (IsDotExpression(out Expression dot))
			{
				e = dot;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsIndexExpression(out Expression index))
			{
				e = index;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsName(out string name))
			{
				e = new Expression()
				{
					type = Expressiontype.name,
					value = name
				};

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsConst(out Expression cnst))
			{
				e = cnst;

				return true;
			}
			currentTokenIndex = savedIndex;
			if (IsEmptyArray(out Expression arr))
			{
				e = arr;

				return true;
			}
			currentTokenIndex = savedIndex;

			return false;
		}

		bool IsRange(out Expression e)
		{
			e = null;

			if (!IsKeyword("range"))
			{
				return false;
			}
			if (!IsSymbol('('))
			{
				return false;
			}
			if (!IsExpression(out Expression expr))
			{
				return false;
			}
			if (!IsSymbol(')'))
			{
				return false;
			}

			e = new Expression()
			{
				type = Expressiontype.range,
				left = expr
			};

			return true;
		}

		bool IsEmptyArray(out Expression e)
		{
			e = null;

			if (!IsSymbol('['))
			{
				return false;
			}
			if (!IsSymbol(']'))
			{
				return false;
			}

			e = new Expression()
			{
				type = Expressiontype.arr
			};

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

		bool IsConst(out Expression e)
		{
			var token = GetCurrentToken;
			e = null;

			if (token.type == LexemType.NUMBER)
			{
				e = new Expression()
				{
					type = Expressiontype.number,
					value = consts[token.constIndex]
				};

				return true;
			}
			else if (token.type == LexemType.STRING)
			{
				e = new Expression()
				{
					type = Expressiontype.str,
					value = consts[token.constIndex]
				};

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
