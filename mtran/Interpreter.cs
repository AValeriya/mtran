using System;
using System.Collections.Generic;

namespace mtran
{
	internal enum StandardType
	{
		TYPE_NONE,
		TYPE_BOOL,
		TYPE_INTEGER,
		TYPE_REAL,
		TYPE_STRING,
		TYPE_ARRAY,
		TYPE_OBJECT,
	}

	internal class RuntimeType
	{
		internal StandardType typeType = StandardType.TYPE_NONE;
	}

	internal class RuntimeValue
	{
		internal RuntimeType runtimeType;
		internal bool boolValue;
		internal long integerValue;
		internal double realValue;
		internal string stringtValue;
		internal List<RuntimeValue> arrayValue;
	}

	internal class Variable
	{
		internal string name;
		internal RuntimeValue value;
	}

	internal class Interpreter
	{
		Ast ast;
		int statementIndex = 0;
		List<string> importedModules;
		List<Variable> variables;

		internal Interpreter(Ast ast)
		{
			this.ast = ast;
			this.importedModules = new List<string>();
			this.variables = new List<Variable>();
		}

		internal bool Run()
		{
			while (statementIndex != -1 && statementIndex < ast.statements.Count)
			{
				var stat = ast.statements[statementIndex];
				var result = InterpretStatement(stat);
				if (!result)
				{
					return result;
				}
				statementIndex++;
			}

			return true;
		}

		private bool InterpretStatement(Statement stat)
		{
			//Console.WriteLine($"Interpeting {stat}");
			switch (stat.statementType)
			{
				case StatementType.STATEMENT_TYPE_IMPORT:
					{
						bool result = InterpretImport(stat as Import);
						if (!result)
						{
							ReportError("", stat);

							return false;
						}
					}
					break;
				case StatementType.STATEMENT_TYPE_ASSIGNMENT:
					{
						bool result = InterpretAssignment(stat as Assignment);
						if (!result)
						{
							ReportError("", stat);

							return false;
						}
					}
					break;
				case StatementType.STATEMENT_TYPE_IF:
					{
						bool result = InterpretIf(stat as If);
						if (!result)
						{
							ReportError("", stat);

							return false;
						}
					}
					break;
				case StatementType.STATEMENT_TYPE_ELSE:
					{
						bool result = InterpretElse(stat as Else);
						if (!result)
						{
							ReportError("", stat);

							return false;
						}
					}
					break;
				case StatementType.STATEMENT_TYPE_FOR:
					{
						bool result = InterpretFor(stat as For);
						if (!result)
						{
							ReportError("", stat);

							return false;
						}
					}
					break;
				case StatementType.STATEMENT_TYPE_WHILE:
					{
						bool result = InterpretWhile(stat as While);
						if (!result)
						{
							ReportError("", stat);

							return false;
						}
					}
					break;
				case StatementType.STATEMENT_TYPE_FUNCTION_CALL:
					{
						bool result = InterpretFunctionCall(stat as FunctionCall);
						if (!result)
						{
							ReportError("", stat);

							return false;
						}
					}
					break;
				default:
				case StatementType.STATEMENT_TYPE_EXPRESSION:
					return false;
			}

			return true;
		}

		private Variable GetVariable(string name)
		{
			var var = variables.Find(v => v.name == name);

			if (var == null)
			{
				var = new Variable()
				{
					name = name,
				};
				variables.Add(var);
			}

			return var;
		}

		private RuntimeValue GetValue(bool b)
		{
			return new RuntimeValue()
			{
				runtimeType = new RuntimeType()
				{
					typeType = StandardType.TYPE_BOOL,
				},
				boolValue = b,
			};
		}

		private RuntimeValue GetValue(int i)
		{
			return new RuntimeValue()
			{
				runtimeType = new RuntimeType()
				{
					typeType = StandardType.TYPE_INTEGER,
				},
				integerValue = i,
			};
		}

		private RuntimeValue GetValue(long l)
		{
			return new RuntimeValue()
			{
				runtimeType = new RuntimeType()
				{
					typeType = StandardType.TYPE_INTEGER,
				},
				integerValue = l,
			};
		}

		private RuntimeValue GetValue(double d)
		{
			return new RuntimeValue()
			{
				runtimeType = new RuntimeType()
				{
					typeType = StandardType.TYPE_REAL,
				},
				realValue = d,
			};
		}

		private RuntimeValue GetValue(Expression expr)
		{
			// TODO

			return null;
		}

		private bool SetValue(Expression expr, RuntimeValue value)
		{
			return true;

			switch (expr.expressionType)
			{
				case Expressiontype.EXPRESSION_TYPE_NAME:
					{
						var name = expr.value;
						var var = GetVariable(name);
						var.value = value;
					}
					return true;
				case Expressiontype.EXPRESSION_TYPE_INDEX:
					return false;
				case Expressiontype.EXPRESSION_TYPE_DOT:
					return false;
				default:
					return false;
			}
		}

		private bool IsTrue(RuntimeValue value)
		{
			if (value == null)
			{
				return true;
			}

			switch (value.runtimeType.typeType)
			{
				case StandardType.TYPE_NONE:
					return false;
				case StandardType.TYPE_BOOL:
					return value.boolValue;
				case StandardType.TYPE_INTEGER:
					return value.integerValue > 0;
				case StandardType.TYPE_REAL:
					return value.realValue > 0.0d;
				case StandardType.TYPE_STRING:
					return value.stringtValue.Length > 0;
				case StandardType.TYPE_ARRAY:
					return value.arrayValue.Count > 0;
				case StandardType.TYPE_OBJECT:
					return true;
				default:
					return false;
			}
		}

		private int GetNextStatementWithIndentationNOrLess(int n)
		{
			for (int i = statementIndex + 1; i < ast.statements.Count; i++)
			{
				var stat = ast.statements[i];
				if (stat.indentation <= n)
				{
					return i;
				}
			}

			return -1;
		}

		private bool InterpretImport(Import importStatement)
		{
			var module = importStatement.library;

			if (!importedModules.Contains(module))
			{
				importedModules.Add(module);
			}

			return true;
		}

		private bool InterpretAssignment(Assignment assignmentStatement)
		{
			var rhs = GetValue(assignmentStatement.right);

			switch (assignmentStatement.assignmentType)
			{
				case AssignmentType.ASSIGNMENT_TYPE_ASSIGNMENT:
					{
						if (!SetValue(assignmentStatement.left, rhs))
						{
							// TODO blahbaslhblahjajh

							return false;
						}
					}
					break;
				case AssignmentType.ASSIGNMENT_TYPE_ADD:
				case AssignmentType.ASSIGNMENT_TYPE_SUB:
				case AssignmentType.ASSIGNMENT_TYPE_MUL:
				case AssignmentType.ASSIGNMENT_TYPE_DIV:
				default:
					break;
			}

			return true;
		}

		private bool InterpretIf(If ifStatement)
		{
			var expr = GetValue(ifStatement.condition);
			if (IsTrue(expr))
			{
				var nextStatementAfterIfIndex = GetNextStatementWithIndentationNOrLess(ifStatement.indentation);
				for (int i = statementIndex + 1; i < nextStatementAfterIfIndex; i++)
				{
					var stat = ast.statements[i];
					statementIndex = i;
					bool result = InterpretStatement(stat);
					if (!result)
					{
						// TODO
					}
				}
			}
			else
			{
				statementIndex = GetNextStatementWithIndentationNOrLess(ifStatement.indentation);
				var nextStatementAfterIf = ast.statements[statementIndex];
				if (nextStatementAfterIf.statementType == StatementType.STATEMENT_TYPE_IF)
				{
					var ifStat = nextStatementAfterIf as If;
					if (ifStat.isElif)
					{
						bool result = InterpretStatement(ifStat);
						if (!result)
						{
							// TODO
						}
						statementIndex = GetNextStatementWithIndentationNOrLess(ifStatement.indentation);
					}
				}
			}

			return true;
		}

		private bool InterpretElse(Else elseStatement)
		{
			var nextStatementAfterIfIndex = GetNextStatementWithIndentationNOrLess(elseStatement.indentation);
			for (int i = statementIndex + 1; i < nextStatementAfterIfIndex; i++)
			{
				var stat = ast.statements[i];
				statementIndex = i;
				bool result = InterpretStatement(stat);
				if (!result)
				{
					// TODO
				}
			}

			return true;
		}

		private bool InterpretFor(For forStatement)
		{
			//while (IsTrue(GetValue(forStatement.expression)))
			if (IsTrue(GetValue(forStatement.expression)))
			{
				var nextStatementAfterIfIndex = GetNextStatementWithIndentationNOrLess(forStatement.indentation);
				for (int i = statementIndex + 1; i < nextStatementAfterIfIndex; i++)
				{
					var stat = ast.statements[i];
					statementIndex = i;
					bool result = InterpretStatement(stat);
					if (!result)
					{
						// TODO
					}
					//statementIndex = nextStatementAfterIfIndex;
				}
			}

			return true;
		}

		private bool InterpretWhile(While whileStatement)
		{
			//while (IsTrue(GetValue(whileStatement.condition)))
			if (IsTrue(GetValue(whileStatement.condition)))
			{
				var nextStatementAfterIfIndex = GetNextStatementWithIndentationNOrLess(whileStatement.indentation);
				for (int i = statementIndex + 1; i < nextStatementAfterIfIndex; i++)
				{
					var stat = ast.statements[i];
					statementIndex = i;
					bool result = InterpretStatement(stat);
					if (!result)
					{
						// TODO
					}
				}
			}

			return true;
		}

		private bool InterpretFunctionCall(FunctionCall functionCallStatement)
		{
			//return true;

			var name = functionCallStatement.left.value;

			if (name == null)
			{
				return true;
			}

			switch (name)
			{
				case "randint":
					{

					}
					return true;
				case "append":
					{

					}
					return true;
				case "print":
					{
						Console.WriteLine(functionCallStatement.parameters[0].ToString());
					}
					return true;
			}

			return true;
		}

		void ReportError(string error, Statement stat)
		{
			Console.Error.WriteLine($"Interpreter error {error} in line {stat.line + 1} ({stat.statementType})");
		}
	}
}
