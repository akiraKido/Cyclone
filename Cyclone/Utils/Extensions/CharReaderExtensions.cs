using Cyclone.Utils;

namespace Cyclone.Template
{
    internal static class CharReaderExtensions
    {
        internal static string GetUntilCode(this CharReader reader)
        {
            return reader.TakeUntil((current, peek) => current == '{' && (peek == '{' || peek == '%'));
        }

        internal static string GetCode(this CharReader reader)
        {
            reader.SkipWhile(c => c == '{' || c == '%');
            var code = reader.TakeUntil((current, peek) => (current == '}' || current == '%') && peek == '}').Trim();
            reader.SkipWhile(c => c == '}' || c == '%');
            return code.Trim();
        }
    }
}