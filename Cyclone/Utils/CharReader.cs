using System;

namespace Cyclone.Utils
{

    internal class CharReader
    {
        private readonly string _text;
        private int _index;

        internal CharReader(string text)
        {
            _text = text;
        }

        internal void Reset() => _index = 0;
        internal bool HasNext => _index < _text.Length;
        internal char Current => HasNext ? _text[_index] : '\0';
        internal bool CanPeek => _index + 1 < _text.Length;
        internal char Peek => CanPeek ? _text[_index + 1] : '\0';

        internal void MoveNext()
        {
            _index++;
            SkipWhiteSpace();
        }

        internal void SkipWhiteSpace() => SkipWhile(char.IsWhiteSpace);

        internal void SkipWhile(Func<char, bool> predicate)
        {
            while (HasNext && predicate(Current))
            {
                MoveNext();
            }
        }

        internal string TakeUntil(Func<char, char, bool> predicate)
        {
            int startIndex = _index;
            while (HasNext && !predicate(Current, Peek))
            {
                _index++;
            }
            var result = _text.Substring(startIndex, _index - startIndex);
            SkipWhiteSpace();
            return result;
        }
        
    }
}
