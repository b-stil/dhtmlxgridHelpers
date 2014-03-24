using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DHXHelperDemo.Models
{
    public class DemoData
    {
        public IEnumerable<DemoDHXVM> GetDemoData()
        {
            List<DemoDHXVM> items;
            using (var reader = new StreamReader(HttpContext.Current.Server.MapPath(@"~/Content/Resources/dataobject.txt")))
            {
                string json = reader.ReadToEnd();
                items = JsonConvert.DeserializeObject<List<DemoDHXVM>>(json);
            }
            return items;
        }
    }
}