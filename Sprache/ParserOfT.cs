using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sprache
{
    public delegate Result<T> Parser<T>(Input input);

    public static class ParserExtensions
    {
        public static Result<T> Parse<T>(this Parser<T> parser, string input)
        {
            Enforce.ArgumentNotNull(parser, "parser");
            Enforce.ArgumentNotNull(input, "input");

            return parser(new Input(input));
        }
    }
}
