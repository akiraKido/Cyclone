using System;
using System.IO;
using Cyclone.Web;

namespace CycloneConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var app = new Application
                {
                    { "/", typeof( MainHandler )}
                };
                app.LogWriter = new LogWriter();
                app.TemplatePath = Path.Combine( Application.ExecutionPath, "Templates" );
                app.Listen("7000");
                app.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
            }
        }
        
    }

    class MainHandler : RequestHandler
    {
        public override void Get()
        {
            //Write("Hello World!");
            Render( "index.html", new {name = "world!"} );
        }
    }

    class LogWriter : ILogWriter
    {
        public void WriteLine(string message)
        {
            Console.WriteLine(message);
        }
    }
}
