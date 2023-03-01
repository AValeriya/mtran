using System;
using System.Collections.Generic;
using System.Globalization;

namespace MyCoolLexer
{
    internal class Lexer
    {
        const string specialSymbols = "(){}[]<>,.:;!@%|&^*-+=/?";

        List<string> names;
        List<string> consts;
        List<Token> tokens;

        LexemType currentTokenType;

        internal List<string> Names => names;
        internal List<string> Consts => consts;
        internal List<Token> Tokens => tokens;

        internal Lexer(string text)
        {
            names = new List<string>();
            consts = new List<string>();
            tokens = new List<Token>();

            Analyse(text);
        }

        void Analyse(string text)
        {
            string temp = "";
            char stringOpening = ' ';

            currentTokenType = LexemType.NONE;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (currentTokenType == LexemType.STRING)
                {
                    var type = GetTokenType(c);
                    if (type == LexemType.STRING)
                    {
                        if (stringOpening != c)
                        {
                            ReportError($"String quotes are inconsistent: {stringOpening} and {c}");
                        }
                        AddConst(LexemType.STRING, temp);
                        temp = "";
                        currentTokenType = LexemType.NONE;
                    }
                    else
                    {
                        temp += c;
                    }
                }
                else
                {
                    if (c == '#')
                    {
                        while (i < text.Length)
                        {
                            c = text[i++];
                            if (c == '\n' || c == '\r')
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        var type = GetTokenType(c);
                        if (type != currentTokenType)
                        {
                            switch (currentTokenType)
                            {
                                case LexemType.NAME:
                                    AddName(temp);
                                    temp = "";
                                    break;
                                case LexemType.NUMBER:
                                    AddConst(LexemType.NUMBER, temp);
                                    temp = "";
                                    break;
                                default:
                                    break;
                            }
                            currentTokenType = type;
                        }

                        switch (type)
                        {
                            case LexemType.NAME:
                            case LexemType.NUMBER:
                                temp += c;
                                break;
                            case LexemType.SPECIAL:
                                tokens.Add(new Token(this, LexemType.SPECIAL, c.ToString()));
                                break;
                            case LexemType.STRING:
                                currentTokenType = LexemType.STRING;
                                stringOpening = c;
                                temp = "";
                                break;
                            case LexemType.NONE:
                            case LexemType.SPACE:
                            default:
                                break;
                        }
                    }
                }
            }

            switch (currentTokenType)
            {
                case LexemType.NAME:
                    AddName(temp);
                    break;
                case LexemType.NUMBER:
                    AddConst(LexemType.NUMBER, temp);
                    break;
                case LexemType.STRING:
                    ReportError($"String did not end!:{temp}");
                    break;
                default:
                    break;
            }
        }

        void AddName(string name)
        {
            int index = names.IndexOf(name);
            if (index == -1)
            {
                names.Add(name);
                tokens.Add(new Token(this, LexemType.NAME, null, nameIndex: names.Count - 1));
            }
            else
            {
                tokens.Add(new Token(this, LexemType.NAME, null, nameIndex: index));
            }
        }

        void AddConst(LexemType type, string value)
        {
            if (type == LexemType.NUMBER)
            {
                if (!double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double d))
                {
                    if (!long.TryParse(value, out long l))
                    {
                        ReportError($"Invalid numeric value: {value}");
                    }
                }
            }
            int index = consts.IndexOf(value);
            if (index == -1)
            {
                consts.Add(value);
                tokens.Add(new Token(this, type, null, constIndex: consts.Count - 1));
            }
            else
            {
                tokens.Add(new Token(this, type, null, constIndex: index));
            }
        }

        void ReportError(string error)
        {
            Console.Error.WriteLine($"Error: {error}");
        }

        LexemType GetTokenType(char c)
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

                return LexemType.NONE;
            }
        }
    }
}