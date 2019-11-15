#region

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

#endregion

namespace Convex.Base.Calculator
{
    public partial class InlineCalculator
    {
        public enum CalcMode
        {
            Numeric,
            Logic
        }

        public static class Token
        {
            public const string P_LEFT = "(",
                P_RIGHT = ")",
                POWER = "^",
                UNARY_MINUS = "_",
                ADD = "+",
                SUBTRACT = "-",
                MULTIPLY = "*",
                DIVIDE = "/",
                FACTORIAL = "!",
                MOD = "%",
                SENTINEL = "#",
                END = ";",
                STORE = "=",
                NONE = " ",
                SEPERATOR = ",";

            public const string SINE = "sin",
                COSINE = "cos",
                TANGENT = "tan",
                A_SINE = "asin",
                A_COSINE = "acos",
                A_TANGENT = "atan",
                LOG = "log",
                LOG10 = "log10",
                LN = "ln",
                EXP = "exp",
                ABS = "abs",
                SQRT = "sqrt",
                ROOT = "rt";

            private static readonly string[] _BinaryOperators =
            {
                MULTIPLY,
                DIVIDE,
                SUBTRACT,
                ADD,
                POWER,
                LOG,
                ROOT,
                MOD
            };

            private static readonly string[] _UnaryOperators =
            {
                SUBTRACT,
                SINE,
                COSINE,
                TANGENT,
                A_SINE,
                A_COSINE,
                A_TANGENT,
                LOG10,
                LN,
                EXP,
                ABS,
                SQRT
            };

            private static readonly string[] _SpecialOperators =
            {
                SENTINEL,
                END,
                STORE,
                NONE,
                SEPERATOR,
                P_RIGHT
            };

            private static readonly string[] _RightSideOperators =
            {
                FACTORIAL
            };

            private static readonly string[] _FunctionList =
            {
                SINE,
                COSINE,
                TANGENT,
                A_SINE,
                A_COSINE,
                A_TANGENT,
                LOG,
                LOG10,
                LN,
                EXP,
                ABS,
                SQRT,
                ROOT
            };

            private static readonly string[] _LastProcessedOperators =
            {
                POWER
            };

            private static int Precedence(string op)
            {
                if (IsFunction(op))
                {
                    return 64;
                }

                return op switch
                {
                    SUBTRACT => 4,
                    ADD => 4,
                    UNARY_MINUS => 8,
                    MULTIPLY => 16,
                    DIVIDE => 16,
                    POWER => 24,
                    MOD => 32,
                    FACTORIAL => 48,
                    P_LEFT => 64,
                    P_RIGHT => 64,
                    _ => 0
                };
            }

            public static int Compare(string op1, string op2)
            {
                if (op1.Equals(op2) && Contains(op1, _LastProcessedOperators))
                {
                    return -1;
                }

                return Precedence(op1) >= Precedence(op2)
                    ? 1
                    : -1;
            }

            public static string ConvertOperator(string op)
            {
                return op switch
                {
                    "-" => "_",
                    _ => op
                };
            }

            public static string ToString(string op)
            {
                return op switch
                {
                    END => "END",
                    _ => op
                };
            }

            private static bool Contains(string token, IEnumerable<string> array)
            {
                return array.Any(s => s.Equals(token));
            }

            #region Is... Functions

            public static bool IsBinary(string op) => Contains(op, _BinaryOperators);

            public static bool IsUnary(string op) => Contains(op, _UnaryOperators);

            public static bool IsRightSide(string op) => Contains(op, _RightSideOperators);

            public static bool IsSpecial(string op) => Contains(op, _SpecialOperators);

            public static bool IsFunction(string op) => Contains(op, _FunctionList);

            public static bool IsName(string token) => Regex.IsMatch(token, @"[a-zA-Z0-9]");

            public static bool IsDigit(string token) => Regex.IsMatch(token, @"\d|\.");

            #endregion
        }
    }
}