using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sprache
{
    public delegate Result<T> Parser<T>(Input input);

    public static class ParserExtensions
    {
        public static Result<T> TryParse<T>(this Parser<T> parser, string input)
        {
            Enforce.ArgumentNotNull(parser, "parser");
            Enforce.ArgumentNotNull(input, "input");

            return parser(new Input(input));
        }

        public static T Parse<T>(this Parser<T> parser, string input)
        {
            Enforce.ArgumentNotNull(parser, "parser");
            Enforce.ArgumentNotNull(input, "input");

            var result = parser.TryParse(input);
            
            var success = result as Success<T>;
            if (success != null)
                return success.Result;

            var failure = (Failure<T>) result;
            throw new ParseException(failure.FailedInput, failure.Message);
        }
    }
}
