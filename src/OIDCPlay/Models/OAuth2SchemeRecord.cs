using System.Collections.Generic;

namespace OIDCPlay.Models
{   /*
      "oauth2": [

         {
           "scheme": "google",
           "authority": "https://accounts.google.com",
           "callbackPath": "/signin-google",
           "additionalEndpointBaseAddresses": []
         }
       ]
    */
    public class OAuth2SchemeRecord
    {
        public string Scheme { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Authority { get; set; }
        public string CallbackPath { get; set; }
        public List<string> AdditionalEndpointBaseAddresses { get; set; }
        public List<string> AcrValues { get; set; }
    }
}