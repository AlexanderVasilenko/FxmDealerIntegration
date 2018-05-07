using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Volvo.Fxm.Cors.Domain
{
    public class CorsClient
    {
        public string Name { get; set; }
        public string HostName { get; set; }
        public string AllowHeaders { get; set; }
        public string Credentials { get; set; }
    }
}