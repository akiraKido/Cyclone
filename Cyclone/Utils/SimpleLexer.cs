using System;
using System.Collections.Generic;

namespace Cyclone.Utils
{
    internal class SimpleLexer
    {
        private readonly string _text;
        private int _index;

        internal SimpleLexer(string text)
        {
            _text = text;
        }

        /// <summary>
        /// The result of Next().
        /// </summary>
        public string Current { get; private set; }

        private char CurrentChar => CanMove ? _text[_index] : '\0';
        private bool CanMove => _index < _text.Length;

        private void SkipWhiteSpace()
        {
            while (CanMove && char.IsWhiteSpace(CurrentChar))
            {
                _index++;
            }
        }

        /// <summary>
        /// Iterates each token.
        /// </summary>
        internal IEnumerable<string> Tokens { get { while (CanMove) yield return Next(); } }
        /// <summary>
        /// Resets the Iterator
        /// </summary>
        internal void Reset() => _index = 0;

        /// <summary>
        /// Checks the next token to be s.
        /// </summary>
        /// <exception cref="Exception">When s doesn't match the next token</exception>
        /// <param name="s">string to check</param>
        internal void ConfirmNext(string s)
        {
            SkipWhiteSpace();
            for (int i = 0; i < s.Length; i++)
            {
                if (!CanMove) throw new Exception($"Expected {s} but reached end of file.");
                if (s[i] != _text[_index + i]) throw new Exception($"Expected {s} but found {Next()}");
            }
            _index += s.Length;
        }

        /// <summary>
        /// Checks the current token to be s.
        /// </summary>
        /// <exception cref="Exception">When s doesn't match the current token.</exception>
        /// <param name="s">string to check</param>
        internal void ConfirmCurrent( string s )
        {
            if ( s != Current ) throw new Exception( $"Expected {s} but found {Current}");
        }

        /// <summary>
        /// Moves to next token.
        /// </summary>
        /// <returns>Equivalent to Current.</returns>
        internal string Next()
        {
            SkipWhiteSpace();
            var start = _index;

            MoveIndexToEndOfToken();

            var length = _index - start;
            Current = start + length >= _text.Length
                ? _text.Substring(start)
                : _text.Substring(start, length);
            return Current;
        }

        private void MoveIndexToEndOfToken()
        {
            var c = CurrentChar;

            if (c.IsVariableStart())
            {
                IncrementWhileVariable();
            }
            else if (char.IsDigit(c))
            {
                IncrementWhileNumeric();
            }
            else if (char.IsSymbol(c))
            {
                IncrementWhileSymbol();
            }
            else if (char.IsPunctuation(c))
            {
                // usually punctuations appear once
                _index++;
                if (char.IsSymbol(CurrentChar))
                    // but in case of != ( ! is a punctuation )
                    _index++;
            }
        }

        private void IncrementWhileVariable()
        {
            _index++;
            while (CurrentChar.IsVariableTrail())
            {
                _index++;
            }
        }

        private void IncrementWhileNumeric()
        {
            bool hasDecimal = false;
            while (true)
            {
                while (char.IsDigit(CurrentChar))
                {
                    _index++;
                }
                if (CurrentChar != '.') break;

                if (hasDecimal) throw new Exception("Second . found in float.");
                hasDecimal = true;
                _index++;
            }
        }

        private void IncrementWhileSymbol()
        {
            while (char.IsSymbol(CurrentChar))
            {
                _index++;
            }
        }

    }
}
