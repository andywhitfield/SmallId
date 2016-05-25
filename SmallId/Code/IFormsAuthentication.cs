using System;

namespace SmallId.Code
{
    public interface IFormsAuthentication
    {
        DateTime? SignedInTimestampUtc { get; }
        void SignIn(string userName, bool createPersistentCookie);
        void SignOut();
    }
}