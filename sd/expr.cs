
using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace sd
{
    public interface IGetVariable
    {
        double GetVariable(string s);
    }

    internal class Expr
    {
        public static int op_precedence(char c)
        {
            Debug.Assert((c == '-') || (c == '+') || (c == '*') || (c == '/'));

            if ((c == '-') || (c == '+'))
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }

        public static int op_precedence(string s)
        {
            return op_precedence(s[0]);
        }

        private static void parse_constant(List<string> toks, string s, ref int cur)
        {
            int start = cur;

            while (
                (cur < s.Length)
                && (char.IsDigit(s, cur)
                || (s[cur] == '.'))
                )
            {
                cur++;
            }

            toks.Add(s.Substring(start, cur - start));
        }

        private static void parse_variable(List<string> toks, string s, ref int cur)
        {
            int start = cur;

            while (
                (cur < s.Length)
                && (char.IsLetterOrDigit(s, cur)
                || (s[cur] == '_'))
                )
            {
                cur++;
            }

            toks.Add(s.Substring(start, cur - start));
        }

        private static string[] lex(string s)
        {
            List<string> toks = new List<string>();

            if (s != null)
            {
                int cur = 0;
                while (cur < s.Length)
                {
                    while (Char.IsWhiteSpace(s, cur))
                    {
                        cur++;
                    }
                    if (char.IsDigit(s, cur))
                    {
                        parse_constant(toks, s, ref cur);
                    }
                    else if (char.IsLetter(s, cur))
                    {
                        parse_variable(toks, s, ref cur);
                    }
                    else if (
                        (s[cur] == '(')
                        || (s[cur] == ')')
                        || (s[cur] == '+')
                        || (s[cur] == '-')
                        || (s[cur] == '*')
                        || (s[cur] == '/')
                        )
                    {
                        toks.Add(s[cur].ToString());
                        cur++;
                    }
                    else
                    {
                        throw new Exception("Illegal character in expression");
                    }
                }
            }

            string[] result = new string[toks.Count];
            toks.CopyTo(result);
            return result;
        }

        public static bool is_operand(string t)
        {
            if (Char.IsDigit(t, 0))
            {
                return true;
            }
            if (Char.IsLetter(t, 0))
            {
                return true;
            }
            return false;
        }

        public static bool is_operator(string t)
        {
            switch (t)
            {
                case "+":
                case "-":
                case "*":
                case "/":
                    {
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        public static void do_op(Stack<double> stk2, string q)
        {
            double d2 = (double)stk2.Pop();
            double d1 = (double)stk2.Pop();
            switch (q)
            {
                case "+":
                    {
                        stk2.Push(d1 + d2);
                        break;
                    }
                case "-":
                    {
                        stk2.Push(d1 - d2);
                        break;
                    }
                case "*":
                    {
                        stk2.Push(d1 * d2);
                        break;
                    }
                case "/":
                    {
                        stk2.Push(d1 / d2);
                        break;
                    }
            }
        }

        public static double get_value(IGetVariable p, string s)
        {
            if (char.IsLetter(s, 0))
            {
                return p.GetVariable(s);
            }
            else
            {
                return double.Parse(s);
            }
        }

        public static double Evaluate(string s, IGetVariable p)
        {
            string[] toks = lex(s);

            Stack<string> stk = new Stack<string>();
            Stack<double> stk2 = new Stack<double>();

            for (int i = 0; i < toks.Length; i++)
            {
                string t = toks[i];
                string tnext = null;
                string tprev = null;
                if ((i + 1) < toks.Length)
                {
                    tnext = toks[i + 1];
                }
                if (i > 0)
                {
                    tprev = toks[i - 1];
                }
                if (t == "(")
                {
                    stk.Push(t);
                }
                else if (t == ")")
                {
                    while (true)
                    {
                        if (stk.Count == 0)
                        {
                            throw new Exception("Unbalanced paren");
                        }
                        string q = (string)stk.Pop();
                        if (q == "(")
                        {
                            break;
                        }
                        else
                        {
                            do_op(stk2, q);
                        }
                    }
                }
                else if (is_operand(t))
                {
                    stk2.Push(get_value(p, t));
                }
                else if (
                    (t == "-")
                    && (tnext != null)
                    && (is_operand(tnext))
                    && (
                        is_operator(tprev)
                        || (
                            (stk.Count == 0)
                            && (stk2.Count == 0)
                            )
                        )
                    )
                {
                    stk2.Push(-get_value(p, tnext));
                    i++;
                }
                else
                {
                    Debug.Assert(is_operator(t));
                    do
                    {
                        if (stk.Count == 0)
                        {
                            stk.Push(t);
                            break;
                        }
                        else if (((string)stk.Peek()) == "(")
                        {
                            stk.Push(t);
                            break;
                        }
                        else if (op_precedence(t) > op_precedence((string)stk.Peek()))
                        {
                            stk.Push(t);
                            break;
                        }
                        else
                        {
                            do_op(stk2, (string)stk.Pop());
                        }
                    } while (true);
                }
            }

            while (stk.Count > 0)
            {
                do_op(stk2, (string)stk.Pop());
            }

            double result = (double)stk2.Pop();

            if (stk2.Count > 0)
            {
                throw new Exception("Stack should be empty at the end of the expression");
            }

            return result;
        }
    }
}
