#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

#endregion

namespace Convex.Base.Calculator
{
    public partial class InlineCalculator
    {
        private string _Expression;

        private Stack<double> _Operands;
        private Stack<string> _Operators;

        private string _Token;
        private int _TokenPos;

        public void Clear()
        {
            _Operands = new Stack<double>();
            _Operators = new Stack<string>();

            _Operators.Push(Token.SENTINEL);
            _Token = Token.NONE;
            _TokenPos = -1;
        }

        public double Evaluate(string toEval)
        {
            Clear();
            LoadConstants();

            _Expression = toEval;

            if (Normalize(ref _Expression))
            {
                double result = Parse();
                SetVariable(ANSWER_VAR, result);
                return result;
            }

            ThrowException("Blank input expression.");
            return 0;
        }

        private double Parse()
        {
            ParseBinary();
            Expect(Token.END);
            return _Operands.Peek();
        }

        private void ParseBinary()
        {
            ParsePrimary();

            while (Token.IsBinary(_Token))
            {
                PushOperator(_Token);
                NextToken();
                ParsePrimary();
            }

            while (_Operators.Peek() != Token.SENTINEL)
            {
                PopOperator();
            }
        }

        private void ParsePrimary()
        {
            while (true)
            {
                if (Token.IsDigit(_Token))
                {
                    ParseDigit();
                }
                else if (Token.IsName(_Token))
                {
                    ParseName();
                }
                else if (Token.IsUnary(_Token))
                {
                    PushOperator(Token.ConvertOperator(_Token));
                    NextToken();
                    continue;
                }
                else
                {
                    switch (_Token)
                    {
                        case Token.P_LEFT:
                            NextToken();
                            _Operators.Push(Token.SENTINEL);
                            ParseBinary();
                            Expect(Token.P_RIGHT, Token.SEPERATOR);
                            _Operators.Pop();

                            TryInsertMultiply();
                            TryRightSideOperator();
                            break;

                        case Token.SEPERATOR:
                            NextToken();
                            continue;
                        default:
                            ThrowException("Syntax error.");
                            break;
                    }
                }

                break;
            }
        }

        private void ParseDigit()
        {
            StringBuilder tmpNumber = new StringBuilder();

            while (Token.IsDigit(_Token))
            {
                CollectToken(ref tmpNumber);
            }

            _Operands.Push(double.Parse(tmpNumber.ToString(), CultureInfo.InvariantCulture));
            TryInsertMultiply();
            TryRightSideOperator();
        }

        private void ParseName()
        {
            StringBuilder tmpName = new StringBuilder();

            while (Token.IsName(_Token))
            {
                CollectToken(ref tmpName);
            }

            string name = tmpName.ToString();

            if (Token.IsFunction(name))
            {
                PushOperator(name);
                ParsePrimary();
            }
            else if (_Token.Equals(Token.STORE))
            {
                NextToken();
                SetVariable(name, Parse());
            }
            else
            {
                _Operands.Push(GetVariable(name));
                TryInsertMultiply();
                TryRightSideOperator();
            }
        }

        private void TryInsertMultiply()
        {
            if (Token.IsBinary(_Token) || Token.IsSpecial(_Token) || Token.IsRightSide(_Token))
            {
                return;
            }

            PushOperator(Token.MULTIPLY);
            ParsePrimary();
        }

        private void TryRightSideOperator()
        {
            switch (_Token)
            {
                case Token.FACTORIAL:
                    PushOperator(Token.FACTORIAL);
                    NextToken();
                    TryInsertMultiply();
                    break;

                case Token.SEPERATOR:
                    ParsePrimary();
                    break;
            }
        }

        private void PushOperator(string op)
        {
            if (Token.UNARY_MINUS.Equals(op))
            {
                _Operators.Push(op);
                return;
            }

            while (Token.Compare(_Operators.Peek(), op) > 0)
            {
                PopOperator();
            }

            _Operators.Push(op);
        }

        private void PopOperator()
        {
            if (Token.IsBinary(_Operators.Peek()))
            {
                double o2 = _Operands.Pop();
                double o1 = _Operands.Pop();
                Calculate(_Operators.Pop(), o1, o2);
            }
            else
            {
                Calculate(_Operators.Pop(), _Operands.Pop());
            }
        }

        private void NextToken()
        {
            if (_Token != Token.END)
            {
                _Token = _Expression[++_TokenPos].ToString();
            }
        }

        private void CollectToken(ref StringBuilder sb)
        {
            sb.Append(_Token);
            NextToken();
        }

        private void Expect(params string[] expectedTokens)
        {
            if (expectedTokens.Any(t => _Token.Equals(t)))
            {
                NextToken();
                return;
            }

            ThrowException($"Syntax error: {Token.ToString(expectedTokens[0])} expected.");
        }

        private bool Normalize(ref string s)
        {
            s = s.Replace(" ", "")
                    .Replace("\t", " ")
                    .ToLower()
                + Token.END;

            if (s.Length < 2)
            {
                return false;
            }

            NextToken();
            return true;
        }

        private void ThrowException(string message)
        {
            throw new CalculateException(message, _TokenPos);
        }
    }

    public class CalculateException : Exception
    {
        public CalculateException(string message, int position) : base($"Error at position: {position}, {message}") =>
            TokenPosition = position;

        public CalculateException()
        {
        }

        public CalculateException(string message) : base(message)
        {
        }

        public CalculateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public int TokenPosition { get; }
    }
}