using System;
using System.Collections.Generic;
using System.Text;

namespace Cyclone.Utils
{
    internal static class CharExtensions
    {
        internal static bool IsVariableStart( this char c )
        {
            return char.IsLetter( c ) || c == '_';
        }

        internal static bool IsVariableTrail( this char c )
        {
            return char.IsLetterOrDigit( c ) || c == '_';
        }
    }
}
