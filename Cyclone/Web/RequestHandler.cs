using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Cyclone.Template;

namespace Cyclone.Web
{
    public abstract class RequestHandler
    {
        internal Application Application { private get; set; }

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

        public void Render<T>( string fileName, T model )
        {
            var templatePath = Path.Combine( Application.TemplatePath, fileName );
            if(!File.Exists( templatePath )) throw new FileNotFoundException(templatePath);

            var result = TemplateBuilder.Build( File.ReadAllText(templatePath), model );
            Content = Content.Concat( Encoding.UTF8.GetBytes(result) ).ToArray();
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
