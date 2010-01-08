﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sprache
{
    /// <summary>
    /// Parsers and combinators.
    /// </summary>
    public static class Parse
    {
        /// <summary>
        /// TryParse a single character matching 'predicate'
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static Parser<char> Char(Predicate<char> predicate, string description)
        {
            Enforce.ArgumentNotNull(predicate, "predicate");
            Enforce.ArgumentNotNull(description, "description");

            return i =>
            {
                if (!i.AtEnd)
                {
                    if (predicate(i.Current))
                    {
                        return Result.Succeed(i.Current, i.Advance());
                    }
                    else
                    {
                        return new Failure<char>(i,
                            "Expected '{0}' but found '{1}'.", description, i.Current);
                    }
                }
                else
                {
                    return new Failure<char>(i,
                        "Unexpected end-of-input reached while looking for '{0}'.", description);
                }
            };
        }

        /// <summary>
        /// TryParse a single character c.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Parser<char> Char(char c)
        {
            return Char(ch => c == ch, c.ToString());
        }

        public static readonly Parser<char> WhiteSpace = Char(char.IsWhiteSpace, "whitespace");
        public static readonly Parser<char> Digit = Char(char.IsDigit, "digit");
        public static readonly Parser<char> Letter = Char(char.IsLetter, "letter");
        public static readonly Parser<char> LetterOrDigit = Char(char.IsLetterOrDigit, "letter or digit");
        public static readonly Parser<char> Lower = Char(char.IsLower, "lowercase letter");
        public static readonly Parser<char> Upper = Char(char.IsUpper, "upper");
        public static readonly Parser<char> Numeric = Char(char.IsNumber, "numeric character");

        /// <summary>
        /// TryParse a string of characters.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Parser<IEnumerable<char>> String(string s)
        {
            Enforce.ArgumentNotNull(s, "s");
            return s
                .Select(Char)
                .Aggregate(Return(Enumerable.Empty<char>()),
                    (a, p) => a.Concat(p.Once()));
        }
        
        /// <summary>
        /// TryParse first, and if successful, then parse second.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Parser<U> Then<T, U>(this Parser<T> first, Func<T, Parser<U>> second)
        {
            Enforce.ArgumentNotNull(first, "first");
            Enforce.ArgumentNotNull(second, "second");

            return i => first(i).IfSuccess(s => second(s.Result)(s.Remainder));
        }

        /// <summary>
        /// TryParse a stream of elements.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static Parser<IEnumerable<T>> Many<T>(this Parser<T> parser)
        {
            Enforce.ArgumentNotNull(parser, "parser");

            return parser.AtLeastOnce().Try().XOr(Return(Enumerable.Empty<T>()));
        }

        /// <summary>
        /// TryParse a stream of elements with at least one item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static Parser<IEnumerable<T>> AtLeastOnce<T>(this Parser<T> parser)
        {
            Enforce.ArgumentNotNull(parser, "parser");

            return parser.Once().Then(t1 => parser.Many().Select(ts => t1.Concat(ts)));
        }

        /// <summary>
        /// TryParse end-of-input.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static Parser<T> End<T>(this Parser<T> parser)
        {
            Enforce.ArgumentNotNull(parser, "parser");

            return i => parser(i).IfSuccess(s =>
                s.Remainder.AtEnd ?
                    (Result<T>)s :
                    new Failure<T>(s.Remainder, "Expected end of input but got '{0}'", s.Remainder.Current));
        }

        /// <summary>
        /// Take the result of parsing, and project it onto a different domain.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="parser"></param>
        /// <param name="convert"></param>
        /// <returns></returns>
        public static Parser<U> Select<T, U>(this Parser<T> parser, Func<T, U> convert)
        {
            Enforce.ArgumentNotNull(parser, "parser");
            Enforce.ArgumentNotNull(convert, "convert");

            return parser.Then(t => Return(convert(t)));
        }

        /// <summary>
        /// TryParse first, then second, returning only the result of second.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Parser<U> IgnoreThen<T, U>(this Parser<T> first, Parser<U> second)
        {
            Enforce.ArgumentNotNull(first, "first");
            Enforce.ArgumentNotNull(second, "second");

            return first.Then(ignored => second);
        }

        /// <summary>
        /// TryParse first, then second, returning only the result of first.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Parser<T> ThenIgnore<T, U>(this Parser<T> first, Parser<U> second)
        {
            Enforce.ArgumentNotNull(first, "first");
            Enforce.ArgumentNotNull(second, "second");

            return first.Then(result => second.Select(ignored => result));
        }

        /// <summary>
        /// TryParse the token, embedded in any amount of whitespace characters.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static Parser<T> Token<T>(this Parser<T> parser)
        {
            Enforce.ArgumentNotNull(parser, "parser");

            return WhiteSpace.Many().IgnoreThen(parser.ThenIgnore(WhiteSpace.Many()));
        }

        /// <summary>
        /// Refer to another parser indirectly. This allows circular compile-time dependency between parsers.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reference"></param>
        /// <returns></returns>
        public static Parser<T> Ref<T>(Func<Parser<T>> reference)
        {
            Enforce.ArgumentNotNull(reference, "reference");

            Parser<T> p = null;

            return i =>
                       {
                           if (p == null)
                               p = reference();

                           if (i.Memos.ContainsKey(p))
                           {
                               var failure = (Failure<T>)i.Memos[p];
                               throw new ParseException(failure.FailedInput, failure.Message);
                           }

                           i.Memos[p] = new Failure<T>(i, "Left recursion in the grammar.");
                           var result = p(i);
                           i.Memos[p] = result;
                           return result;
                       };
        }

        /// <summary>
        /// Convert a stream of characters to a string.
        /// </summary>
        /// <param name="characters"></param>
        /// <returns></returns>
        public static Parser<string> Text(this Parser<IEnumerable<char>> characters)
        {
            return characters.Select(chs => new string(chs.ToArray()));
        }

        /// <summary>
        /// TryParse first, if it succeeds, return first, otherwise try second.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Parser<T> Or<T>(this Parser<T> first, Parser<T> second)
        {
            return first.Try().XOr(second);
        }

        /// <summary>
        /// TryParse first, if it succeeds, return first, otherwise try second.
        /// Assumes that the first parsed character will determine the parser chosen (see Try).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Parser<T> XOr<T>(this Parser<T> first, Parser<T> second)
        {
            Enforce.ArgumentNotNull(first, "first");
            Enforce.ArgumentNotNull(second, "second");

            return i => {
                var fr = first(i);
                if (fr is Failure<T>)
                {
                    if (((Failure<T>)fr).FailedInput == i)
                        return second(i);
                    else
                        return fr;
                }
                else
                {
                    if (((Success<T>)fr).Remainder == i)
                        return second(i).IfFailure(f => fr);
                    else
                        return fr;
                }
            };
        }

        /// <summary>
        /// If parser fails, treat this as a failure at the first input position.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static Parser<T> Try<T>(this Parser<T> parser)
        {
            Enforce.ArgumentNotNull(parser, "parser");

            return i => parser(i).IfFailure(f => new Failure<T>(i, f.Message));
        }

        /// <summary>
        /// TryParse a stream of elements containing only one item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static Parser<IEnumerable<T>> Once<T>(this Parser<T> parser)
        {
            Enforce.ArgumentNotNull(parser, "parser");

            return parser.Select(r => (IEnumerable<T>)new[] { r });
        }

        /// <summary>
        /// Concatenate two streams of elements.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Parser<IEnumerable<T>> Concat<T>(this Parser<IEnumerable<T>> first, Parser<IEnumerable<T>> second)
        {
            Enforce.ArgumentNotNull(first, "first");
            Enforce.ArgumentNotNull(second, "second");

            return first.Then(f => second.Select(s => f.Concat(s)));
        }

        /// <summary>
        /// Succeed immediately and return value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Parser<T> Return<T>(T value)
        {
            return i => Result.Succeed(value, i);
        }

        /// <summary>
        /// Version of Return with simpler inline syntax.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="parser"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Parser<U> Return<T, U>(this Parser<T> parser, U value)
        {
            return parser.Select(t => value);
        }

        /// <summary>
        /// Succeed if the parsed value matches predicate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parser"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static Parser<T> Where<T>(this Parser<T> parser, Func<T, bool> predicate)
        {
            Enforce.ArgumentNotNull(parser, "parser");
            Enforce.ArgumentNotNull(predicate, "predicate");

            return i => parser(i).IfSuccess(s =>
                predicate(s.Result) ? (Result<T>)s : new Failure<T>(i, "Unexpected {0}.", s.Result));
        }

        /// <summary>
        /// Monadic combinator Then, adapted for Linq comprehension syntax.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="parser"></param>
        /// <param name="selector"></param>
        /// <param name="projector"></param>
        /// <returns></returns>
        public static Parser<V> SelectMany<T, U, V>(
            this Parser<T> parser,
            Func<T, Parser<U>> selector,
            Func<T, U, V> projector)
        {
            Enforce.ArgumentNotNull(parser, "parser");
            Enforce.ArgumentNotNull(projector, "projector");
            Enforce.ArgumentNotNull(selector, "selector");

            return parser.Then(t => selector(t).Select(u => projector(t, u)));
        }

        /// <summary>
        /// Chain a left-associative operator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TOp"></typeparam>
        /// <param name="op"></param>
        /// <param name="operand"></param>
        /// <param name="apply"></param>
        /// <returns></returns>
        public static Parser<T> ChainOperator<T, TOp>(
            Parser<TOp> op,
            Parser<T> operand,
            Func<TOp, T, T, T> apply)
        {
            return operand.Then(first => ChainOperatorRest(first, op, operand, apply));
        }

        static Parser<T> ChainOperatorRest<T, TOp>(
            T firstOperand,
            Parser<TOp> op,
            Parser<T> operand,
            Func<TOp, T, T, T> apply)
        {
            return op.Then(opvalue =>
                    operand.Then(operandValue =>
                        ChainOperatorRest(apply(opvalue, firstOperand, operandValue), op, operand, apply)))
                .XOr(Return(firstOperand));
        }

        /// <summary>
        /// Chain a right-associative operator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TOp"></typeparam>
        /// <param name="op"></param>
        /// <param name="operand"></param>
        /// <param name="apply"></param>
        /// <returns></returns>
        public static Parser<T> ChainRightOperator<T, TOp>(
            Parser<TOp> op,
            Parser<T> operand,
            Func<TOp, T, T, T> apply)
        {
            return operand.Then(first => ChainRightOperatorRest(first, op, operand, apply));
        }

        static Parser<T> ChainRightOperatorRest<T, TOp>(
            T lastOperand,
            Parser<TOp> op,
            Parser<T> operand,
            Func<TOp, T, T, T> apply)
        {
            return op.Then(opvalue =>
                    operand.Then(operandValue =>
                        ChainRightOperatorRest(operandValue, op, operand, apply)).Then(r =>
                            Return(apply(opvalue, lastOperand, r))))
                .XOr(Return(lastOperand));
        }

        public static readonly Parser<string> Number = Numeric.AtLeastOnce().Text();

        public static readonly Parser<string> Decimal =
            from integral in Parse.Number
            from fraction in Parse.Char('.').IgnoreThen(Number.Select(n => "." + n)).XOr(Return(""))
            select integral + fraction;

    }
}
