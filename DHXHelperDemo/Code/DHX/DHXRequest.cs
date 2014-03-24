using System;
using System.Web;

namespace DHXHelperDemo.Code.DHX
{
    public static class DHXRequest
    {
        public static bool ValidateDhxRequest(HttpRequestBase httpRequest)
        {
            
            // parse the echo property (must be returned as int to prevent XSS-attack)
            bool wasSuccessful = !String.IsNullOrWhiteSpace(httpRequest.Params["a_dhx_rSeed"]);

            return wasSuccessful;
        }
    }
}