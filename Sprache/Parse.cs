using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sprache
{
    public static class Parse
    {
        public static Parser<IEnumerable<char>> Char(Predicate<char> predicate, string description)
        {
            return i =>
            {
                if (!i.AtEnd)
                {
                    if (predicate(i.Current))
                    {
                        return new Success<IEnumerable<char>>(
                            new[] { i.Current }, i.Advance());
                    }
                    else
                    {
                        return new Failure<IEnumerable<char>>(i,
                            "Expected '{0}' but found '{1}'.", description, i.Current);
                    }
                }
                else
                {
                    return new Failure<IEnumerable<char>>(i,
                        "Unexpected end-of-input reached while looking for '{0}'.", description);
                }
            };
        }

        public static Parser<IEnumerable<char>> Char(char c)
        {
            return Char(ch => c == ch, c.ToString());
        }

        public static readonly Parser<IEnumerable<char>> WhiteSpace = Char(char.IsWhiteSpace, "whitespace");
        public static readonly Parser<IEnumerable<char>> Digit = Char(char.IsDigit, "digit");
        public static readonly Parser<IEnumerable<char>> Letter = Char(char.IsLetter, "letter");
        public static readonly Parser<IEnumerable<char>> LetterOrDigit = Char(char.IsLetterOrDigit, "letter or digit");
        public static readonly Parser<IEnumerable<char>> Lower = Char(char.IsLower, "lowercase letter");
        public static readonly Parser<IEnumerable<char>> Upper = Char(char.IsUpper, "upper");
        public static readonly Parser<IEnumerable<char>> Numeric = Char(char.IsNumber, "numeric character");

        public static Parser<U> Then<T, U>(this Parser<T> first, Func<T, Parser<U>> second)
        {
            return i => first(i).IfSuccess(s => second(s.Result)(s.Remainder));
        }

        public static Parser<V> Combine<T, U, V>(this Parser<T> first, Parser<U> second, Func<T, U, V> combine)
        {
            return i => first(i).IfSuccess(s1 =>
                second(s1.Remainder).IfSuccess(s2 =>
                    new Success<V>(combine(s1.Result, s2.Result), s2.Remainder)));
        }

        public static Parser<IEnumerable<T>> Concat<T>(this Parser<IEnumerable<T>> first, Parser<IEnumerable<T>> second)
        {
            return first.Then(t1 => second.Cast(t2 => t1.Concat(t2)));
        }

        public static Parser<IEnumerable<T>> Repeat<T>(this Parser<IEnumerable<T>> parser)
        {
            return i =>
            {
                var remainder = i;
                var result = Enumerable.Empty<T>();
                Success<IEnumerable<T>> s;
                while ((s = parser(remainder) as Success<IEnumerable<T>>) != null)
                {
                    result = result.Concat(s.Result);
                    remainder = s.Remainder;
                }
                return Result.Succeed(result, remainder);
            };
        }

        public static Parser<T> End<T>(this Parser<T> parser)
        {
            return i => parser(i).IfSuccess(s =>
                s.Remainder.AtEnd ?
                    (Result<T>)s :
                    new Failure<T>(s.Remainder, "Expected end of input but got '{0}'", s.Remainder.Current));
        }

        public static Parser<U> Cast<T, U>(this Parser<T> parser, Func<T, U> convert)
        {
            return i => parser(i).IfSuccess(s => Result.Succeed(convert(s.Result), s.Remainder));
        }

        public static Parser<IEnumerable<T>> Ignore<T>(this Parser<IEnumerable<T>> parser)
        {
            return i => parser(i).IfSuccess(s => Result.Succeed(Enumerable.Empty<T>(), s.Remainder));
        }

        public static Parser<IEnumerable<T>> AtLeastOnce<T>(this Parser<IEnumerable<T>> parser)
        {
            return parser.Concat(parser.Repeat());
        }

        public static Parser<U> IgnoreThen<T, U>(this Parser<T> first, Parser<U> second)
        {
            return first.Then(ignored => second);
        }

        public static Parser<T> ThenIgnore<T, U>(this Parser<T> first, Parser<U> second)
        {
            return first.Then(result => second.Cast(ignored => result));
        }

        public static Parser<string> Token(this Parser<IEnumerable<char>> parser)
        {
            return WhiteSpace.Repeat().IgnoreThen(
                parser.Cast(chrs => new string(chrs.ToArray())).ThenIgnore(
                    WhiteSpace.Repeat()));
        }

        public static Parser<T> Ref<T>(Func<Parser<T>> reference)
        {
            return i => reference()(i);
        }

        public static Parser<T> Or<T>(this Parser<T> left, Parser<T> right)
        {
            return i => left(i).IfFailure(f => f.Input == i ? right(i) : f);
        }

        public static Parser<T> Try<T>(this Parser<T> parser)
        {
            return i => parser(i).IfFailure(f => new Failure<T>(i, f.Message));
        }

        public static Parser<IEnumerable<T>> AsItem<T>(this Parser<T> parser)
        {
            return parser.Cast(r => (IEnumerable<T>)new[] { r });
        }
    }
}
