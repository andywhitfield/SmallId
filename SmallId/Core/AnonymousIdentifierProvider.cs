using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace SmallId.Core
{
    public class AnonymousIdentifierProvider : PrivatePersonalIdentifierProviderBase
    {
        internal AnonymousIdentifierProvider() : base("anon?id=".GetAppPathRootedUri())
        {
        }

        protected override byte[] GetHashSaltForLocalIdentifier(Identifier localIdentifier)
        {
            var membership = (ReadOnlyXmlMembershipProvider)Membership.Provider;
            string username = User.GetUserFromClaimedIdentifier(new Uri(localIdentifier));
            return Convert.FromBase64String(membership.GetSalt(username));
        }
    }
}