using Microsoft.Maui.Controls;
using oop_lab_1_cokcell.Classes.Cells;
using oop_lab_1_cokcell.Classes.Exceptions;
using oop_lab_1_cokcell.Classes.ExpressionNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;

namespace oop_lab_1_cokcell.Classes.Parser
{
    internal static class ExpressionParser
    {
        private const string AllowedMathSymbols = "+-*/,^";
        private static Dictionary<string, Func<Expression>> notParenthesesOperationCodesUnary =
        new Dictionary<string, Func<Expression>>{
                {"+", () => new UnaryPlusNode() },
                {"-", () => new UnaryMinusNode() },
        };

        private static Dictionary<string, Func<Expression>> notParenthesesOperationCodes =
            new Dictionary<string, Func<Expression>>{
                {"+", () => new AddNode() },
                {"-", () => new SubtractNode() },
                {"*", () => new MultiplyNode() },
                {"/", () => new DivideNode() },
            };

        private static Dictionary<string, Func<List<Expression>, Expression>> parenthesesOperationCodes =
            new Dictionary<string, Func<List<Expression>, Expression>>{
                {"abs", args => new AbsoluteNode(args) },
                {"mod", args => new ModuleNode(args) },
            };

        private static void SkipWhitespace(string s, ref int p)
        {
            for (; p < s.Length && char.IsWhiteSpace(s[p]); p++) {}
        }

        private static bool InvalidChar(string s, ref int p)
        {
            return (
                p < s.Length && 
                !(
                    char.IsLetterOrDigit(s[p]) ||
                    char.IsWhiteSpace(s[p]) ||
                    AllowedMathSymbols.Contains(s[p])
                )
            );
        }

        private static char Peek(string s, ref int p, bool toCheckIfValid = false)
        {
            SkipWhitespace(s, ref p);
            if (p >= s.Length) return '\0';
            if (toCheckIfValid && InvalidChar(s, ref p))
                throw new SyntaxError($"Unrecognised character '{s[p]}'", s, p);
            return s[p];
        }

        private static char Read(string s, ref int p, bool toCheckIfValid = false)
        {
            Peek(s, ref p, toCheckIfValid);
            if (p >= s.Length) return '\0';
            return s[p++];
        }

        private static string ReadObjectString(string s, ref int p, bool toCheckIfValid = false)
        {
            string ans = "";

            // 1 sized chars
            ans += Read(s, ref p, toCheckIfValid);
            if (AllowedMathSymbols.Contains(ans[0]))
                return ans;

            for (; p < s.Length &&
                !char.IsWhiteSpace(s[p]) &&
                !char.IsPunctuation(s[p]) &&
                !char.IsBetween(s[p], '(', ')') &&
                !AllowedMathSymbols.Contains(s[p]);)
            {
                ans += Read(s, ref p, toCheckIfValid);
            }
            return ans;
        }

        private static Expression ConvertStringIntoObject(string obj,
            string globalString, ref int globalP, TableAction cellReferenceInit,
            bool isUnary, out bool isValueNode, List<CellId> linkables)
        {
            if(isUnary)
            {
                if (notParenthesesOperationCodesUnary.ContainsKey(obj.ToLower()))
                {
                    isValueNode = false;
                    return notParenthesesOperationCodesUnary[obj.ToLower()]();
                }
            }

            if (notParenthesesOperationCodes.ContainsKey(obj.ToLower()))
            {
                isValueNode = false;
                return notParenthesesOperationCodes[obj.ToLower()]();
            }

            if (parenthesesOperationCodes.ContainsKey(obj.ToLower()))
            {
                isValueNode = true;
                return parenthesesOperationCodes[obj.ToLower()]
                    (ParseArguments(globalString, ref globalP, cellReferenceInit, linkables));
            }

            if (obj.All(x => char.IsDigit(x)))
            {
                isValueNode = true;
                return new NumberNode(BigInteger.Parse(obj));
            }

            // TO ADD: ReferenceNode
            if (obj.All(x => char.IsLetterOrDigit(x)
                && (!char.IsLetter(x)
                || char.IsUpper(x))))
            {
                isValueNode = true;
                return new ReferenceNode(new CellId(obj), cellReferenceInit);
            }

            if (obj.Length == 1)
                throw new SyntaxError(
                    $"Cannot identify {obj} object, on location {globalP - obj.Length}",
                    globalString, globalP - obj.Length);
            else
                throw new SyntaxError(
                    $"Cannot identify {obj} object, on location [{globalP - obj.Length},{globalP}]",
                    globalString, globalP - obj.Length, globalP);
        }

        private static List<Expression> ParseArguments(string s, ref int p, TableAction cellReferenceInit, List<CellId> linkables)
        {
            List<Expression> exp = new List<Expression>();
            if (Peek(s, ref p) == '(') Read(s, ref p); // start (
            else throw new SyntaxError($"Unopened () operator : Expected '(', but got '{Peek(s, ref p)}' at position {p}", s, p);

            while (p < s.Length)
                {
                    if (Peek(s, ref p) == ')') break;
                    else if (Peek(s, ref p) == ',') Read(s, ref p);
                    exp.Add(ParseParentheses(s, ref p, cellReferenceInit, linkables));
                }
            if (Peek(s, ref p) == ')') Read(s, ref p); // start (
            else throw new SyntaxError($"Unclosed () operator : Expected ')', but got '{Peek(s, ref p)}' at position {p}", s, p);
            return exp;
        }

        private static Expression ParseParentheses(string s, ref int p, TableAction cellReferenceInit, List<CellId> linkables)
        {
            List<Expression> exp = new List<Expression>();
            int cntParentheses = 0;
            bool isUnaryContext = true;
            while (p < s.Length && cntParentheses >= 0)
            {
                if (Peek(s, ref p) == ')' || Peek(s, ref p) == ',') break;
                while (Peek(s, ref p) == '(') { cntParentheses++; Read(s, ref p); }

                if (cntParentheses != 0)
                {
                    exp.Add(ParseParentheses(s, ref p, cellReferenceInit, linkables));
                    isUnaryContext = false;

                    for (; cntParentheses > 0; cntParentheses--)
                    {
                        if (Peek(s, ref p) != ')')
                            throw new SyntaxError(
                                string.Format("Expected ')', but got '{0}' at position {1}",
                                (p < s.Length) ? s[p] : '\0', p), s, p);
                        Read(s, ref p);
                    }
                }
                else
                {
                    var x = ConvertStringIntoObject(
                            ReadObjectString(s, ref p, true), s, ref p, cellReferenceInit,
                            isUnaryContext, out bool isValueNode, linkables
                    );

                    if (x is ReferenceNode node)
                        linkables.Add(node.GetKey());

                    exp.Add(x);

                    // work with
                    //  2 +-+3 
                    // ->> (probably)
                    //  2 + (-(+3))
                    isUnaryContext = !isValueNode;
                }
            }

            if(cntParentheses != 0)
                throw new SyntaxError(
                    $"Unclosed parentheses : Expected ')', but got '{s[p]}' at position {p}", s, p);

            for (int i = exp.Count - 2; i >= 0; i--)
            {
                if (exp[i] is OperationNode op
                    && op.Priority == 1 && !op.Ready()
                    // opt 
                    && op.NeededArgs == 1
                    )
                {
                    op.Arguments = new List<Expression> { exp[i + 1] };
                    exp.RemoveAt(i + 1);
                }
            }

            for (int prior = 2; prior <= 3; prior++) {
                for (int i = 1; i < exp.Count - 1; i++)
                {
                    if (exp[i] is OperationNode op
                    && op.Priority == prior && !op.Ready()
                    // opt 
                    && op.NeededArgs == 2)
                    {
                        op.Arguments = new List<Expression> { exp[i - 1], exp[i + 1] };
                        exp[i - 1] = exp[i];
                        exp.RemoveRange(i, 2);

                        i--;
                    }
                }
            }

            if (exp.Count != 1)
            {
                string operators = "";
                foreach (var x in exp)
                    if (x is OperationNode op)
                        operators += op.NameLog+'\n';
                throw new SyntaxError($"Invalid argument count for 1 of operators:\n{operators}");
            }
            return exp[0];
        }

        public static Expression Parse(string s, TableAction cellReferenceInit, out List<CellId> linkables)
        {
            linkables = new List<CellId>();
            if (s == string.Empty) return new Expression();

            int pos = 0;
            var ans = ParseParentheses(s, ref pos, cellReferenceInit, linkables);
            // for endl ')'
            if (pos != s.Length)
                throw new SyntaxError($"Extra character : {s[pos]} at position {pos}", s, pos);
            return ans;
        }
    }
}
