# Cyclone

This is a Tornado clone written in C#.

To use Cyclone, import the Cyclone C# project to your project (for now).

- Supports .NET Standard 2.0

# Hello, World!

The following is the current Hello world to Cyclone.

```csharp

using System;
using Cyclone.Web;

namespace CycloneConsole
{

    class MainHandler : RequestHandler
    {
        public override void Get()
        {
            Write("Hello World!");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var app = new Application();
                app.AddRoute("/", typeof(MainHandler));
                app.Listen("7000");
                app.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
    
}


```

Checkout Cyclone/CycloneConsole for a working sample.
