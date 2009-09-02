using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sprache
{
    static class Enforce
    {
        public static T ArgumentNotNull<T>(T arg, string argumentName)
        {
            if (arg == null)
                throw new ArgumentNullException(argumentName);

            return arg;
        }
    }
}
