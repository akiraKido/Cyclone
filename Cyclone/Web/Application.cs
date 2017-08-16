using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;

namespace Cyclone.Web
{

    public class Application : IEnumerable
    {
        public ILogWriter LogWriter { private get; set; }

        private readonly HttpListener _listener;
        private readonly List<RouteOption> _handlers = new List<RouteOption>();
        private readonly IRequestProcessor _requestProcessor;

        public Application(ServerOptions serverOptions = null)
        {
            if(serverOptions == null) serverOptions = new ServerOptions();

            _listener = new HttpListener();
            _requestProcessor = serverOptions.MultiThread
                ? (IRequestProcessor)new MultiThreadRequestProcessor(_handlers)
                : new SingleThreadRequestProcessor(_handlers);
        }

        public void AddRoute(string path, Type requestHandler)
        {
            Add(path, requestHandler);
        }

        public void Add(string path, Type requestHandler)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (requestHandler == null) throw new ArgumentNullException(nameof(requestHandler));

            if (!requestHandler.IsSubclassOf(typeof(RequestHandler)))
            {
                throw new Exception(requestHandler + " is not a valid Request Handler");
            }

            _handlers.Add(new RouteOption { Route = path, RequestHandler = requestHandler });
        }


        public void Listen(string port)
        {
#if DEBUG
            _listener.Prefixes.Add($"http://localhost:{port}/");
#else
            // IP port open needs administrator privilege
            _listener.Prefixes.Add($"http://*:{port}/");
#endif
        }

        public void Start()
        {
            try
            {
                _listener.Start();

                while (true)
                {
                    HttpListenerContext context = _listener.GetContext();
                    LogWriter?.WriteLine($"{DateTime.Now}\t{context.Request.HttpMethod}\t{context.Request.Url}");
                    _requestProcessor.Process(context);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }

        public IEnumerator GetEnumerator() => throw new NotSupportedException();
    }
}
