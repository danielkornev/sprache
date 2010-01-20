using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sprache
{
    public abstract class Result<T>
    {
        public abstract Result<U> IfSuccess<U>(Func<Success<T>, Result<U>> next);
        public abstract Result<T> IfFailure(Func<Failure<T>, Result<T>> next);
    }

    public static class Result
    {
        public static Success<T> Succeed<T>(T result, Input remainder)
        {
            return new Success<T>(result, remainder);
        }
    }

    public sealed class Success<T> : Result<T>
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

        public override Result<U> IfSuccess<U>(Func<Success<T>, Result<U>> next)
        {
            return next(this);
        }

        public override Result<T> IfFailure(Func<Failure<T>, Result<T>> next)
        {
            return this;
        }

        public override string ToString()
        {
            return string.Format("Successful parsing of {0}.", Result);
        }
    }

    public sealed class Failure<T> : Result<T>
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

        public override Result<U> IfSuccess<U>(Func<Success<T>, Result<U>> next)
        {
            return new Failure<U>(FailedInput, _message, _expectations);
        }

        public override Result<T> IfFailure(Func<Failure<T>, Result<T>> next)
        {
            return next(this);
        }

        public Result<U> ChangeType<U>()
        {
            return new Failure<U>(_input, _message, _expectations);
        }

        public override string ToString()
        {
            string expMsg = "";
            
            if (Expectations.Any())
                expMsg = " expected " + Expectations.Aggregate((e1, e2) => e1 + " or " + e2);
            
            return string.Format("Parsing failure: {0};{1} ({2}).", Message, expMsg, FailedInput);
        }
    }
}
