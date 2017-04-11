using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication2
{
    static public class Scanner
    {

        public struct Token
        {
            public string TokenValue;
            public string TokenType;
        }

        static public SortedDictionary<string, string> Tokens = new SortedDictionary<string, string>();

        static public List<Token> Result = new List<Token>();

        static public void InitiateTokens()
        {
            Tokens.Add("if", "IF");
            Tokens.Add("then", "THEN");
            Tokens.Add("else", "ELSE");
            Tokens.Add("end", "END");
            Tokens.Add("repeat", "REPEAT");
            Tokens.Add("until", "UNTIL");
            Tokens.Add("read", "READ");
            Tokens.Add("write", "WRITE");
            Tokens.Add("+", "PLUS");
            Tokens.Add("-", "MINUS");
            Tokens.Add("*", "TIMES");
            Tokens.Add("/", "OVER");
            Tokens.Add("=", "EQ");
            Tokens.Add("<", "LT");
            Tokens.Add("(", "LPAREN");
            Tokens.Add(")", "RPAREN");
            Tokens.Add(";", "SEMI");
            Tokens.Add(":=", "ASSIGN");
        }

        static public string Translate(string element)
        {
            foreach (KeyValuePair<string, string> kv in Tokens)
            {
                if (element == kv.Key)
                    return kv.Value;
            }

            return null;
        }

        public enum Types
        {
            SpecialSymbols, Number, Letter, Null, Comment
        }

        static public void Scan(string Code)
        {
            Token Current;
            Types Memory = Types.Null;
            string element = "";

            for (int i = 0; i < Code.Length; i++)
            {
                char x = Code[i];
                //in case we are inside a comment
                if (element == "{")
                {
                    if (x == '}')
                    {
                        element = "";
                        continue;
                    }
                    else
                        continue;
                }


                else
                {

                    if (x == '{')
                    {
                        element = "{";
                        Memory = Types.Comment;
                        continue;
                    }

                    else if (x == ' ' || x == '\n' || x == '\t')
                    {
                        if (element != "")
                        {
                            if (Memory == Types.Number)
                            {
                                Current.TokenValue = element;
                                Current.TokenType = "NUM";
                                Result.Add(Current);
                            }

                            else if (Memory == Types.Letter)
                            {
                                string temp = null;
                                temp = Translate(element);
                                if (temp == null)
                                {
                                    Current.TokenValue = element;
                                    Current.TokenType = "ID";
                                    Result.Add(Current);
                                }
                                else
                                {
                                    Current.TokenValue = element;
                                    Current.TokenType = temp;
                                    Result.Add(Current);
                                }

                            }

                            element = "";
                            Memory = Types.Null;
                        }

                        //element = "" 
                        else
                            continue;

                    }

                    else if (x == '+' || x == '-' || x == '*' || x == '/' || x == '<' || x == '(' || x == ')' || x == ';' || x == ':')
                    {
                        if (Memory == Types.Number)
                        {
                            Current.TokenValue = element;
                            Current.TokenType = "NUM";
                            Result.Add(Current);
                        }

                        else if (Memory == Types.Letter)
                        {
                            string temp = null;
                            temp = Translate(element);
                            if (temp == null)
                            {
                                Current.TokenValue = element;
                                Current.TokenType = "ID";
                                Result.Add(Current);
                            }
                            else
                            {
                                Current.TokenValue = element;
                                Current.TokenType = temp;
                                Result.Add(Current);
                            }
                        }

                        if (x == ':')
                        {
                            Memory = Types.SpecialSymbols;
                            element = ":";
                            continue;
                        }
                        else
                        {
                            Memory = Types.SpecialSymbols;
                            Current.TokenValue = x.ToString();
                            Current.TokenType = Translate(x.ToString());
                            Result.Add(Current);
                            element = "";
                        }

                    }


                    else if (x == '=')
                    {
                        if (element == ":")
                        {
                            element += x;
                            Current.TokenValue = element;
                            Current.TokenType = Translate(element);
                            Result.Add(Current);
                            Memory = Types.SpecialSymbols;
                        }
                        else
                        {
                            if (Memory == Types.Number)
                            {
                                Current.TokenValue = element;
                                Current.TokenType = "NUM";
                                Result.Add(Current);
                            }

                            else if (Memory == Types.Letter)
                            {
                                string temp = null;
                                temp = Translate(element);
                                if (temp == null)
                                {
                                    Current.TokenValue = element;
                                    Current.TokenType = "ID";
                                    Result.Add(Current);
                                }
                                else
                                {
                                    Current.TokenValue = element;
                                    Current.TokenType = temp;
                                    Result.Add(Current);
                                }

                            }

                            Current.TokenValue = x.ToString();
                            Current.TokenType = Translate(x.ToString());
                            Result.Add(Current);

                        }

                        element = "";
                    }

                    else if (Char.IsNumber(x))
                    {
                        Memory = Types.Number;
                        element += x;
                    }
                    else if (Char.IsLetter(x))
                    {
                        Memory = Types.Letter;
                        element += x;
                    }

                    //last char or letter in the code
                    if (i == Code.Length - 1)
                    {
                        if (Memory == Types.Number)
                        {
                            Current.TokenValue = element;
                            Current.TokenType = "NUM";
                            Result.Add(Current);
                        }

                        else if (Memory == Types.Letter)
                        {
                            string temp = null;
                            temp = Translate(element);
                            if (temp == null)
                            {
                                Current.TokenValue = element;
                                Current.TokenType = "ID";
                                Result.Add(Current);
                            }
                            else
                            {
                                Current.TokenValue = element;
                                Current.TokenType = temp;
                                Result.Add(Current);
                            }
                        }

                        Memory = Types.Null;

                    }

                }

            }


        }

    }
}
