using System;
using Cyclone.Web;

namespace CycloneConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var app = new Application();
                app.LogWriter = new LogWriter();
                app.AddRoute("/", typeof(MainHandler));
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
            Write("Hello World!");
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
