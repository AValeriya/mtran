using System.Collections.Generic;

namespace mtran
{
	internal class Ast
	{
		internal List<Statement> statements;

		public override string ToString()
		{
			string result = "";

			foreach (var stat in statements)
			{
				for (int i = 0; i < stat.indentation; i++)
				{
					result += "    ";
				}
				result += stat.ToString() + '\n';
			}

			return result;
		}
	}

	internal class Statement
	{
		internal int indentation;
	}

	internal class Import : Statement
	{
		internal string library;
		internal string name;

		public override string ToString()
		{
			return $"Importing {name} from {library} library";
		}
	}

	internal class Assignment : Statement
	{
		internal Expression left;
		internal Expression right;
		internal AssignmentType type;

		public override string ToString()
		{
			switch (type)
			{
				case AssignmentType.assignment:
					return $"Assigning {left} = {right}";
				case AssignmentType.add:
					return $"Assigning {left} += {right}";
				case AssignmentType.sub:
					return $"Assigning {left} -= {right}";
				case AssignmentType.mul:
					return $"Assigning {left} *= {right}";
				case AssignmentType.div:
					return $"Assigning {left} /= {right}";
				default:
					return "???error???";
			}
		}
	}

	internal class If : Statement
	{
		internal Expression condition;
		internal bool isElif = false;

		public override string ToString()
		{
			return $"If statement: {condition}";
		}
	}

	internal class Else : Statement
	{
		public override string ToString()
		{
			return $"Else branch:";
		}
	}

	internal class For : Statement
	{
		internal Expression variable;
		internal Expression range;

		public override string ToString()
		{
			return $"For statement: {variable} in {range}";
		}
	}

	internal class While : Statement
	{
		internal Expression condition;

		public override string ToString()
		{
			return $"while statement: {condition}";
		}
	}

	internal class Expression : Statement
	{
		internal Expressiontype type;
		internal Expression left;
		internal Expression right;
		internal string value;

		public override string ToString()
		{
			if (type == Expressiontype.sub && right == null)
			{
				return $"-{left}";
			}

			switch (type)
			{
				case Expressiontype.name:
					return $"{value}";
				case Expressiontype.number:
					return $"{value}";
				case Expressiontype.str:
					return $"\'{value}\'";
				case Expressiontype.range:
					return $"range({left})";
				case Expressiontype.arr:
					return $"[]"; // TODO
				case Expressiontype.sub:
					return $"{left} - {right}";
				case Expressiontype.add:
					return $"{left} + {right}";
				case Expressiontype.mul:
					return $"{left} * {right}";
				case Expressiontype.div:
					return $"{left} / {right}";
				case Expressiontype.dot:
					return $"{left}.{right}";
				case Expressiontype.index:
					return $"{left}[{right}]";
				case Expressiontype.less:
					return $"{left} < {right}";
				case Expressiontype.lessOrEquals:
					return $"{left} <= {right}";
				case Expressiontype.greater:
					return $"{left} > {right}";
				case Expressiontype.greaterOrEquals:
					return $"{left} >= {right}";
				case Expressiontype.equals:
					return $"{left} == {right}";
				case Expressiontype.or:
					return $"{left} | {right}";
				case Expressiontype.and:
					return $"{left} & {right}";
				case Expressiontype.xor:
					return $"{left} ^ {right}";
				default:
					return "???error???";
			}
		}
	}

	internal class FunctionCall : Expression
	{
		internal List<Expression> parameters;

		public override string ToString()
		{
			string args = "";

			for (int i = 0; i < parameters.Count; i++)
			{
				if (i > 0)
				{
					args += ", ";
				}
				var a = parameters[i];
				args += a.ToString();
			}

			return $"calling function {left} with ({args})";
		}
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
}