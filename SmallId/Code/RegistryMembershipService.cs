namespace SmallId.Code
{
    public class RegistryMembershipService : IMembershipService
    {
        public bool ValidateUser(string userName, string password)
        {
            // TODO: a proper implementation actually checking against the Registry
            return userName == "andy" && password == "good";
        }
    }
}