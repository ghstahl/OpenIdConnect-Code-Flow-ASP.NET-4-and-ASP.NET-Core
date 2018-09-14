using IdentityModel.Client;

namespace Microsoft.Owin.Security.OpenIdConnect
{
    public class GenericDiscoverCacheContainer : DiscoverCacheContainer
    {
        private DiscoveryCache _discoveryCache { get; set; }
        private OpenIdConnectAuthenticationOptions Options { get; set; }

        public GenericDiscoverCacheContainer(OpenIdConnectAuthenticationOptions options)
        {
            Options = options;

        }
        public override DiscoveryCache DiscoveryCache
        {
            get
            {
                if (_discoveryCache == null)
                {
                    var authority = Options.Authority;

                    var discoveryClient = new DiscoveryClient(authority)
                    {
                        Policy = { ValidateEndpoints = false }
                    };

                    _discoveryCache = new DiscoveryCache(discoveryClient);
                }
                return _discoveryCache;
            }
        }
    }
}