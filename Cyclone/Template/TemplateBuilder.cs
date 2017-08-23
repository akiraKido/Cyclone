using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Cyclone.Template
{
    public abstract class TemplateBuilder
    {
        public abstract string Build<T>(string template, T model);

        protected static object GetField<T>(T obj, string field)
        {
            var factory = Cache.GetOrAdd($"{nameof(T)}_{field}", _ =>
                {
                    var target = Expression.Parameter(typeof(object), "target");
                    var convert = Expression.Convert(target, typeof(T));
                    var propertyOrField = Expression.PropertyOrField(convert, field);
                    var lambda = Expression.Lambda<Func<object, object>>(propertyOrField, target);
                    return lambda.Compile();
                });
            return factory(obj);
        }
        private static readonly ConcurrentDictionary<string, Func<object, object>> Cache
            = new ConcurrentDictionary<string, Func<object, object>>();
    }
}
