using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Provider;
using System.Collections.Concurrent;

namespace SmallId.Code
{
    internal class AnonymousIdentifierProvider : PrivatePersonalIdentifierProviderBase
    {
        private readonly ConcurrentDictionary<Identifier, byte[]> salts = new ConcurrentDictionary<Identifier, byte[]>();

        internal AnonymousIdentifierProvider()
            : base(Util.GetAppPathRootedUri("anon?id="))
        {
        }

        protected override byte[] GetHashSaltForLocalIdentifier(Identifier localIdentifier)
        {
            return salts.GetOrAdd(localIdentifier, _ => CreateSalt());
        }
    }
}