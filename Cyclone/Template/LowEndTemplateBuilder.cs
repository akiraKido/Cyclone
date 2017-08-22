using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Cyclone.Utils;

namespace Cyclone.Template
{
    public sealed class LowEndTemplateBuilder : ITemplateBuilder
    {
        public static readonly LowEndTemplateBuilder Instance = new LowEndTemplateBuilder();

        private LowEndTemplateBuilder() { }

        public string Build<T>(string template, T model)
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

        private static object GetField<T>(T obj, string field)
        {
            var target = Expression.Parameter(typeof(object), "target");

            var lambda = _cache.GetOrAdd($"{nameof(T)}_{field}",
                Expression.Lambda<Func<object, object>>
                (
                    Expression.PropertyOrField
                    (
                        Expression.Convert(target, typeof(T)),
                        field
                    ),
                    target
                ).Compile());
            return lambda(obj);
        }

        private static readonly ConcurrentDictionary<string, Func<object, object>> _cache = new ConcurrentDictionary<string, Func<object, object>>();
    }
}
