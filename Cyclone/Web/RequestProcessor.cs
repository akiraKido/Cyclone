using System.Net;
using System.Threading.Tasks;

namespace Cyclone.Web
{
    internal interface IRequestProcessor
    {
        void Process(RequestHandler handler, HttpListenerContext context);
    }

    internal abstract class RequestProcessorBase : IRequestProcessor
    {
        public void ProcessRequest(RequestHandler handler, HttpListenerContext context)
        {
            HttpListenerResponse response = context.Response;

            handler.Handle( context.Request );
            byte[] content = handler.Content;
            response.OutputStream.Write(content, 0, content.Length);

            response.Close();
        }

        public abstract void Process(RequestHandler handler, HttpListenerContext context);
    }

    internal class SingleThreadRequestProcessor : RequestProcessorBase
    {
        public override void Process(RequestHandler handler, HttpListenerContext context)
        {
            ProcessRequest(handler, context);
        }
    }

    internal class MultiThreadRequestProcessor : RequestProcessorBase
    {
        public override void Process(RequestHandler handler, HttpListenerContext context)
        {
            Task.Run(() => {ProcessRequest(handler, context);});
        }
    }
}
