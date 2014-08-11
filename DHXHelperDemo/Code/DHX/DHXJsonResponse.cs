using System.Collections.Generic;

namespace DHXHelperDemo.Code.DHX
{
    public class DHXJsonResponse<T>
    {
        public int total_count { get; set; }
        public int pos { get; set; }
        public List<T> rows { get; set; }
        
    }
}