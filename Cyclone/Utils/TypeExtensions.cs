using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Cyclone.Web;

namespace Cyclone.Utils
{
    internal static class TypeExtensions
    {
        internal static RequestHandler Instantiate(this Type type)
        {
            var instanceFactory = InstanceFactoryCache.GetOrAdd(type.FullName, _ =>
                Expression.Lambda<Func<RequestHandler>>
                (
                    Expression.Convert
                    (
                        Expression.New(type),
                        typeof(RequestHandler)
                    )
                ).Compile());
            return instanceFactory();
        }
        private static readonly ConcurrentDictionary<string, Func<RequestHandler>> InstanceFactoryCache
            = new ConcurrentDictionary<string, Func<RequestHandler>>();
    }
}
