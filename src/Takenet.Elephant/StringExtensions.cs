using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;

namespace Takenet.Elephant
{
    public static class StringExtensions
    {
        public static string ToStringOfCulture(this object obj, CultureInfo culture)
        {
            return
                (obj as IConvertible)?.ToString(culture) ??
                ((obj as IFormattable)?.ToString(null, culture) ??
                obj.ToString());
        }

        public static string ToStringInvariant(this object obj)
            => ToStringOfCulture(obj, CultureInfo.InvariantCulture);

        /// <summary>
        /// Returns the first few characters of the string with a length
        /// specified by the given parameter. If the string's length is less than the 
        /// given length the complete string is returned. If length is zero or 
        /// less an empty string is returned
        /// </summary>
        /// <param name="s">the string to process</param>
        /// <param name="length">Number of characters to return</param>
        /// <returns></returns>
        public static string Left(this string s, int length)
        {
            length = Math.Max(length, 0);

            return s.Length > length ? s.Substring(0, length) : s;
        }

        /// <summary>
        /// Format the string using the source object to populate the named formats.
        /// http://haacked.com/archive/2009/01/14/named-formats-redux.aspx/
        /// </summary>
        /// <param name="format"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string Format(this string format, object source)
        {
            if (format == null) throw new ArgumentNullException(nameof(format));            
            StringBuilder result = new StringBuilder(format.Length * 2);

            using (var reader = new StringReader(format))
            {
                StringBuilder expression = new StringBuilder();
                int @char = -1;

                State state = State.OutsideExpression;
                do
                {
                    switch (state)
                    {
                        case State.OutsideExpression:
                            @char = reader.Read();
                            switch (@char)
                            {
                                case -1:
                                    state = State.End;
                                    break;
                                case '{':
                                    state = State.OnOpenBracket;
                                    break;
                                case '}':
                                    state = State.OnCloseBracket;
                                    break;
                                default:
                                    result.Append((char)@char);
                                    break;
                            }
                            break;
                        case State.OnOpenBracket:
                            @char = reader.Read();
                            switch (@char)
                            {
                                case -1:
                                    throw new FormatException();
                                case '{':
                                    result.Append('{');
                                    state = State.OutsideExpression;
                                    break;
                                default:
                                    expression.Append((char)@char);
                                    state = State.InsideExpression;
                                    break;
                            }
                            break;
                        case State.InsideExpression:
                            @char = reader.Read();
                            switch (@char)
                            {
                                case -1:
                                    throw new FormatException();
                                case '}':
                                    result.Append((string) OutExpression(source, expression.ToString()));
                                    expression.Length = 0;
                                    state = State.OutsideExpression;
                                    break;
                                default:
                                    expression.Append((char)@char);
                                    break;
                            }
                            break;
                        case State.OnCloseBracket:
                            @char = reader.Read();
                            switch (@char)
                            {
                                case '}':
                                    result.Append('}');
                                    state = State.OutsideExpression;
                                    break;
                                default:
                                    throw new FormatException();
                            }
                            break;
                        default:
                            throw new InvalidOperationException("Invalid state.");
                    }
                } while (state != State.End);
            }

            return result.ToString();
        }

        private static string OutExpression(object source, string expression)
        {
            string format = "";

            int colonIndex = expression.IndexOf(':');
            if (colonIndex > 0)
            {
                format = expression.Substring(colonIndex + 1);
                expression = expression.Substring(0, colonIndex);
            }

            try
            {
                if (String.IsNullOrEmpty(format))
                {
                    return (DataBinder.Eval(source, expression) ?? "").ToString();
                }
                return DataBinder.Eval(source, expression, "{0:" + format + "}") ?? "";
            }
            catch (HttpException)
            {
                throw new FormatException();
            }
        }

        private enum State
        {
            OutsideExpression,
            OnOpenBracket,
            InsideExpression,
            OnCloseBracket,
            End
        }
    }
}