using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sprache
{
    public static class Parse
    {
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

        public static Parser<U> Then<T, U>(this Parser<T> first, Func<T, Parser<U>> second)
        {
            Enforce.ArgumentNotNull(first, "first");
            Enforce.ArgumentNotNull(second, "second");

            return i => first(i).IfSuccess(s => second(s.Result)(s.Remainder));
        }

        public static Parser<IEnumerable<T>> Repeat<T>(this Parser<T> parser)
        {
            Enforce.ArgumentNotNull(parser, "parser");

            return parser.AtLeastOnce().Try().Or(Return(Enumerable.Empty<T>()));
        }

        public static Parser<IEnumerable<T>> AtLeastOnce<T>(this Parser<T> parser)
        {
            Enforce.ArgumentNotNull(parser, "parser");

            return parser.Once().Then(t1 => parser.Repeat().Select(ts => t1.Concat(ts)));
        }

        public static Parser<T> End<T>(this Parser<T> parser)
        {
            Enforce.ArgumentNotNull(parser, "parser");

            return i => parser(i).IfSuccess(s =>
                s.Remainder.AtEnd ?
                    (Result<T>)s :
                    new Failure<T>(s.Remainder, "Expected end of input but got '{0}'", s.Remainder.Current));
        }

        public static Parser<U> Select<T, U>(this Parser<T> parser, Func<T, U> convert)
        {
            Enforce.ArgumentNotNull(parser, "parser");
            Enforce.ArgumentNotNull(convert, "convert");

            return parser.Then(t => Return(convert(t)));
        }

        public static Parser<U> IgnoreThen<T, U>(this Parser<T> first, Parser<U> second)
        {
            Enforce.ArgumentNotNull(first, "first");
            Enforce.ArgumentNotNull(second, "second");

            return first.Then(ignored => second);
        }

        public static Parser<T> ThenIgnore<T, U>(this Parser<T> first, Parser<U> second)
        {
            Enforce.ArgumentNotNull(first, "first");
            Enforce.ArgumentNotNull(second, "second");

            return first.Then(result => second.Select(ignored => result));
        }

        public static Parser<string> Token(this Parser<char> parser)
        {
            Enforce.ArgumentNotNull(parser, "parser");

            return parser.Once().Token();
        }

        public static Parser<string> Token(this Parser<IEnumerable<char>> parser)
        {
            Enforce.ArgumentNotNull(parser, "parser");

            return WhiteSpace.Repeat().IgnoreThen(
                parser.Select(chrs => new string(chrs.ToArray())).ThenIgnore(
                    WhiteSpace.Repeat()));
        }

        public static Parser<T> Ref<T>(Func<Parser<T>> reference)
        {
            Enforce.ArgumentNotNull(reference, "reference");

            return i => reference()(i);
        }

        public static Parser<T> Or<T>(this Parser<T> first, Parser<T> second)
        {
            Enforce.ArgumentNotNull(first, "first");
            Enforce.ArgumentNotNull(second, "second");

            return i => first(i).IfFailure(f => f.Input == i ? second(i) : f);
        }

        public static Parser<T> Try<T>(this Parser<T> parser)
        {
            Enforce.ArgumentNotNull(parser, "parser");

            return i => parser(i).IfFailure(f => new Failure<T>(i, f.Message));
        }

        public static Parser<IEnumerable<T>> Once<T>(this Parser<T> parser)
        {
            Enforce.ArgumentNotNull(parser, "parser");

            return parser.Select(r => (IEnumerable<T>)new[] { r });
        }

        public static Parser<IEnumerable<T>> Concat<T>(this Parser<IEnumerable<T>> first, Parser<IEnumerable<T>> second)
        {
            Enforce.ArgumentNotNull(first, "first");
            Enforce.ArgumentNotNull(second, "second");

            return first.Then(f => second.Select(s => f.Concat(s)));
        }

        public static Parser<T> Return<T>(T value)
        {
            return i => Result.Succeed(value, i);
        }

        public static Parser<T> Where<T>(this Parser<T> parser, Func<T, bool> predicate)
        {
            Enforce.ArgumentNotNull(parser, "parser");
            Enforce.ArgumentNotNull(predicate, "predicate");

            return i => parser(i).IfSuccess(s =>
                predicate(s.Result) ? (Result<T>)s : new Failure<T>(i, "Unexpected {0}.", s.Result));
        }

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
    }
}
