using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace Microsoft.Owin.Security.OpenIdConnect
{
    public abstract class DiscoverCacheContainer
    {
        public abstract DiscoveryCache DiscoveryCache { get; }
    }
}
