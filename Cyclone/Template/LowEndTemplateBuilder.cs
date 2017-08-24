using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Cyclone.Utils;

namespace Cyclone.Template
{
    internal sealed class LowEndTemplateBuilder : TemplateBuilder
    {
        public override string Build<T>(string template, T model)
        {
            // If no models exist, just give back the template.
            if (model == null) return template;

            string result = string.Empty;

            var reader = new CharReader(template);

            while (reader.HasNext)
            {
                result += reader.TakeUntil((c, next) => c == '{' && (next == '{' || next == '%'));
                reader.SkipWhile(c => c == '{' || c == '%');
                string code = reader.TakeUntil((c, next) => (c == '}' || c == '%') && next == '}').Trim();
                if (!string.IsNullOrWhiteSpace(code))
                {
                    object paramResult = GetField(model, code);
                    result += paramResult.ToString();
                }
                reader.SkipWhile(c => c == '}' || c == '%');
            }

            return result;
        }

    }
}
