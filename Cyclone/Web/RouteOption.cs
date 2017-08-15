using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cyclone.Web
{
    internal class RouteOption
    {
        private Regex _route;

        public Regex RouteRegex => _route;

        public string Route
        {
            get => _route.ToString();
            set
            {
                string regexString = value;
                if (regexString.First() != '^')
                    regexString = "^" + regexString;
                if (regexString.Last() != '$')
                    regexString += "$";
                _route = new Regex(regexString);
            }
        }

        public Type RequestHandler;

    }
}
