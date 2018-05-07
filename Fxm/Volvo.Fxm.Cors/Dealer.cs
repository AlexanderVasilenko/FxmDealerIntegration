using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volvo.Fxm.Domain
{
    public class Dealer
    {
        public string Name { get; set; }
        public string Host { get; set; }

        public Dealer(string name, string host)
        {
            this.Name = name;
            this.Host = Host;
        }
    }
}
