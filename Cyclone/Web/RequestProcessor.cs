using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Cyclone.Utils;

namespace Cyclone.Web
{
    internal interface IRequestProcessor
    {
        void Process(HttpListenerContext context);
    }

    internal abstract class RequestProcessorBase : IRequestProcessor
    {
        private sealed class DefaultErrorHandler : RequestHandler
        {
            internal DefaultErrorHandler() { WriteError(); }
        }

        private readonly IEnumerable<RouteOption> _handlers;

        internal RequestProcessorBase(IEnumerable<RouteOption> handlers)
        {
            _handlers = handlers;
        }

        public void ProcessRequest(HttpListenerContext context)
        {
            HttpListenerResponse response = context.Response;
            RouteOption handlerRouteOption = 
                _handlers.FirstOrDefault
                (
                    item => item.RouteRegex.Match(context.Request.Url.LocalPath).Success
                );

            RequestHandler handler;
            if (handlerRouteOption != null)
            {
                response.StatusCode = 200;
                handler = handlerRouteOption.RequestHandler.Instantiate();
                handler.Handle(context.Request);
            }
            else
            {
                response.StatusCode = 404;
                handler = new DefaultErrorHandler();
            }

            var content = handler.Content;
            response.OutputStream.Write(content, 0, content.Length);

            response.Close();
        }

        public abstract void Process(HttpListenerContext context);
    }

    internal class SingleThreadRequestProcessor : RequestProcessorBase
    {
        public SingleThreadRequestProcessor(IEnumerable<RouteOption> handlers) : base(handlers) { }

        public override void Process(HttpListenerContext context)
        {
            ProcessRequest(context);
        }
    }

    internal class MultiThreadRequestProcessor : RequestProcessorBase
    {
        public MultiThreadRequestProcessor(IEnumerable<RouteOption> handlers) : base(handlers) { }

        public override void Process(HttpListenerContext context)
        {
            Task.Run(() => {ProcessRequest(context);});
        }
    }
}
