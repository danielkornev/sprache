using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sprache
{
    public class Input
    {
        readonly string _source;
        readonly int _position;

        public Input(string source)
            : this(source, 0)
        {
        }

        Input(string source, int position)
        {
            _source = source;
            _position = position;
        }

        public Input Advance()
        {
            return new Input(_source, _position + 1);
        }

        public char Current { get { return _source[_position]; } }

        public bool AtEnd { get { return _position == _source.Length; } }

        public override string ToString()
        {
            return "Position = " + _position;
        }
    }
}
