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
            Enforce.ArgumentNotNull(remainder, "remainder");

            return new Success<T>(result, remainder);
        }
    }

    public sealed class Success<T> : Result<T>
    {
        readonly Input _remainder;
        readonly T _result;

        public Success(T result, Input remainder)
        {
            Enforce.ArgumentNotNull(remainder, "remainder");

            _result = result;
            _remainder = remainder;
        }

        public T Result { get { return _result; } }

        public Input Remainder { get { return _remainder; } }

        public override Result<U> IfSuccess<U>(Func<Success<T>, Result<U>> next)
        {
            Enforce.ArgumentNotNull(next, "next");
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
        readonly Input _input;

        public Failure(Input input, string message, params object[] formatArgs)
            : this(input, () => string.Format(message, formatArgs))
        {
            Enforce.ArgumentNotNull(message, "message");
            Enforce.ArgumentNotNull(formatArgs, "formatArgs");
        }

        public Failure(Input input, Func<string> message)
        {
            Enforce.ArgumentNotNull(input, "input");
            Enforce.ArgumentNotNull(message, "message");
            _input = input;
            _message = message;
        }

        public string Message { get { return _message(); } }

        public Input FailedInput { get { return _input; } }

        public override Result<U> IfSuccess<U>(Func<Success<T>, Result<U>> next)
        {
            Enforce.ArgumentNotNull(next, "next");
            return new Failure<U>(FailedInput, () => Message);
        }

        public override Result<T> IfFailure(Func<Failure<T>, Result<T>> next)
        {
            Enforce.ArgumentNotNull(next, "next");
            return next(this);
        }

        public override string ToString()
        {
            return string.Format("Failed parsing: {0} ({1}).", Message, FailedInput);
        }
    }
}
