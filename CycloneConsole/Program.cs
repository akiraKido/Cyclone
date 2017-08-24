using System;
using System.Collections.Generic;
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
                app.TemplatePath = Path.Combine(Application.ExecutionPath, "Templates");
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
            var List = new List<string> { "a", "b", "c" };
            Render("index.html", new Hoge{ name = "world!", list = List, token = null });
        }
    }

    class Hoge
    {
        public string name { get; set; }
        public IEnumerable<string> list { get; set; }
        public string token { get; set; }
    }

    class LogWriter : ILogWriter
    {
        public void WriteLine(string message)
        {
            Console.WriteLine(message);
        }
    }
}
