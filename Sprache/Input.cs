using System;
using System.Collections.Generic;

namespace Sprache
{
    public class Input
    {
        public string Source { get; set; }
        readonly string _source;
        readonly int _position;

        internal IDictionary<object, object> Memos = new Dictionary<object, object>();

        public Input(string source)
            : this(source, 0)
        {
        }

        internal Input(string source, int position)
        {
            Source = source;

            _source = source;
            _position = position;
        }

        public Input Advance()
        {
            if (AtEnd)
                throw new InvalidOperationException("The input is already at the end of the source.");

            return new Input(_source, _position + 1);
        }

        public char Current { get { return _source[_position]; } }

        public bool AtEnd { get { return _position == _source.Length; } }

        public int Position { get { return _position; } }

        public override string ToString()
        {
            return "Position " + _position;
        }

        public override bool Equals(object obj)
        {
            var i = obj as Input;
            return i != null && i._source == _source && i._position == _position;
        }

        public override int GetHashCode()
        {
            return _source.GetHashCode() ^ _position.GetHashCode();
        }
    }
}
