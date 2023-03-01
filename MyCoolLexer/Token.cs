namespace MyCoolLexer
{
    internal class Token
    {
        internal Lexer lexer;
        internal LexemType type;
        internal string symbol;
        internal int nameIndex;
        internal int constIndex;

        internal Token(Lexer lexer, LexemType type, string symbol, int nameIndex = -1, int constIndex = -1)
        {
            this.lexer = lexer;
            this.type = type;
            this.symbol = symbol;
            this.nameIndex = nameIndex;
            this.constIndex = constIndex;
        }

        public override string ToString()
        {
            if (nameIndex != -1)
            {
                return $"Token (type = {type},\tname = \"{lexer.Names[nameIndex]}\")";
            }
            else
            {
                if (constIndex != -1)
                {
                    return $"Token (type = {type},\tconst = \"{lexer.Consts[constIndex]}\")";
                }
                else
                {
                    return $"Token (type = {type},\tsymbol = '{symbol}')";
                }
            }
        }
    }
}