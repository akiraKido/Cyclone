using System;

namespace Cyclone.Utils
{
    enum SkipOptions
    {
        NoSkip,
        SkipWhiteSpace,
    }

    internal class CharReader
    {
        private readonly string _text;
        private readonly SkipOptions _skipOptions;

        private int _index;

        internal CharReader(string text, SkipOptions skipOptions = SkipOptions.SkipWhiteSpace)
        {
            _text = text;
            _skipOptions = skipOptions;
        }

        internal void Reset() => _index = 0;
        internal bool HasNext => _index < _text.Length;
        internal char Current => HasNext ? _text[_index] : '\0';
        internal bool CanPeek => _index + 1 < _text.Length;
        internal char Peek => CanPeek ? _text[_index + 1] : '\0';
        private string CurrentString => Current == '\0' ? "EOF" : Current.ToString();

        internal void MoveNext()
        {
            _index++;
            PerformSkip();
        }

        internal void MoveNext(int count)
        {
            _index += count;
            PerformSkip();
        }

        private void PerformSkip()
        {
            if (_skipOptions == SkipOptions.SkipWhiteSpace) SkipWhiteSpace();
        }

        internal void SkipWhiteSpace() => SkipWhile(char.IsWhiteSpace);

        internal void SkipWhile(Func<char, bool> predicate)
        {
            while (HasNext && predicate(Current))
            {
                MoveNext();
            }
        }

        internal string TakeWhile(Func<char, bool> predicate)
        {
            int startIndex = _index;
            while (HasNext && predicate(Current))
            {
                _index++;
            }
            var result = _text.Substring(startIndex, _index - startIndex);
            return result;
        }

        internal string TakeWhile(Func<char, char, bool> predicate)
        {
            int startIndex = _index;
            while (HasNext && predicate(Current, Peek))
            {
                _index++;
            }
            var result = _text.Substring(startIndex, _index - startIndex);
            return result;
        }

        internal string TakeUntil(Func<char, char, bool> predicate)
        {
            int startIndex = _index;
            while (HasNext && !predicate(Current, Peek))
            {
                _index++;
            }
            var result = _text.Substring(startIndex, _index - startIndex);
            return result;
        }

        internal void Eat( char c )
        {
            if(Current != c) throw new Exception($"Expected: {c} but found {CurrentString}");
            MoveNext();
        }

        internal void Eat( string s )
        {
            PerformSkip();

            int i = 0;
            while ( i < s.Length )
            {
                if(Current != s[i]) throw new Exception($"For: {s}, expected: {s[i]}, but found {CurrentString}");
                MoveNext();
                i += 1;
            }
        }

        internal string PeekWhile( Func<char, bool> predicate )
        {
            int startIndex = _index;
            int length = 0;
            while ( startIndex + length < _text.Length && predicate( _text[startIndex + length] ) )
            {
                length += 1;
            }
            return _text.Substring( startIndex, length );
        }
        
    }
}
