namespace SmallId.Code
{
    public interface IMembershipService
    {
        /// <summary>
        /// Validates the user.
        /// </summary>
        /// <param name="username">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <returns>Whether the given username and password is correct.</returns>
        bool ValidateUser(string username, string password);
        bool RegisterNewUser(string username, string password, string email);
    }
}