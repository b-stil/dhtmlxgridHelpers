using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DHXHelperDemo.Models
{
    public class GridVM<T>
    {
        public int id { get; set; }
        public T data { get; set; } 
    }
}