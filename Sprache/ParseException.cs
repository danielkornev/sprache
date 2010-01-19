using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sprache
{
    public class ParseException : Exception
    {
        public ParseException(Input failedInput, string message)
            : base("Position " + failedInput.Position + ": " + message)
        {
            FailedInput = failedInput;
        }

        public Input FailedInput { get; private set; }
    }
}
