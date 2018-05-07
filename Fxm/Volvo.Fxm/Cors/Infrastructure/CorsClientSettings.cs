using Sitecore.Configuration;
using Sitecore.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using Volvo.Fxm.Cors.Domain;

namespace Volvo.Fxm.Cors.Infrastructure
{
    public class CorsClientSettings
    {
        public CorsClient GetCorsClient(string origin)
        {
            var clients = GetCorsClients();
            return clients.FirstOrDefault(x => x.HostName == origin);
        }

        private List<CorsClient> GetCorsClients()
        {
            List<CorsClient> list = new List<CorsClient>();
            foreach(XmlNode node in Factory.GetConfigNodes("corsClients/dealer"))
            {
                CorsClient cl = new CorsClient
                {
                    Name = XmlUtil.GetAttribute("name", node),
                    HostName = XmlUtil.GetAttribute("hostName", node),
                    AllowHeaders = XmlUtil.GetAttribute("allowHeaders", node),
                    Credentials = XmlUtil.GetAttribute("credentials", node)
                };
                list.Add(cl);
            }
            return list;
        }
    }
}