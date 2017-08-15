using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Cyclone.Web
{
    public abstract class RequestHandler
    {
        internal byte[] Content { get; private set; } = { };

        public virtual void Get() { WriteError(); }
        
        public virtual void Post() { WriteError(); }

        public void Write(string message)
        {
            Content = Content.Concat(Encoding.UTF8.GetBytes(message)).ToArray();
        }

        public virtual void WriteError()
        {
            Write( "404 - Page not found" );
        }

        internal void Handle( HttpListenerRequest request )
        {
            switch (request.HttpMethod)
            {
                case "GET" : Get();         break;
                case "POST": Post();        break;
                default    : WriteError();  break;
            }
        }
    }
}
