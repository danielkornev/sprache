using System;
using System.Collections.Generic;
using System.Linq;

namespace Sprache
{
    public interface IResult<out T>
    {
    }

    interface IResultHelper<T>
    {
        IResult<U> IfSuccess<U>(Func<Success<T>, IResult<U>> next);
        IResult<T> IfFailure(Func<Failure<T>, IResult<T>> next);
    }

    static class ResultHelper
    {
        public static IResult<U> IfSuccess<T, U>(this IResult<T> result, Func<Success<T>, IResult<U>> next)
        {
            return ((IResultHelper<T>) result).IfSuccess(next);
        }

        public static IResult<T> IfFailure<T>(this IResult<T> result, Func<Failure<T>, IResult<T>> next)
        {
            return ((IResultHelper<T>) result).IfFailure(next);
        }
    }

    public sealed class Success<T> : IResult<T>, IResultHelper<T>
    {
        readonly Input _remainder;
        readonly T _result;

        public Success(T result, Input remainder)
        {
            _result = result;
            _remainder = remainder;
        }

        public T Result { get { return _result; } }

        public Input Remainder { get { return _remainder; } }

        IResult<U> IResultHelper<T>.IfSuccess<U>(Func<Success<T>, IResult<U>> next)
        {
            return next(this);
        }

        IResult<T> IResultHelper<T>.IfFailure(Func<Failure<T>, IResult<T>> next)
        {
            return this;
        }

        public override string ToString()
        {
            return string.Format("Successful parsing of {0}.", Result);
        }
    }

    public sealed class Failure<T> : IResult<T>, IResultHelper<T>
    {
        readonly Func<string> _message;
        readonly Func<IEnumerable<string>> _expectations;
        readonly Input _input;

        public Failure(Input input, Func<string> message, Func<IEnumerable<string>> expectations)
        {
            _input = input;
            _message = message;
            _expectations = expectations;
        }

        public string Message { get { return _message(); } }

        public IEnumerable<string> Expectations { get { return _expectations(); } }

        public Input FailedInput { get { return _input; } }

        IResult<U> IResultHelper<T>.IfSuccess<U>(Func<Success<T>, IResult<U>> next)
        {
            return new Failure<U>(FailedInput, _message, _expectations);
        }

        IResult<T> IResultHelper<T>.IfFailure(Func<Failure<T>, IResult<T>> next)
        {
            return next(this);
        }

        public override string ToString()
        {
            var expMsg = "";
            
            if (Expectations.Any())
                expMsg = " expected " + Expectations.Aggregate((e1, e2) => e1 + " or " + e2);
            
            return string.Format("Parsing failure: {0};{1} ({2}).", Message, expMsg, FailedInput);
        }
    }
}
