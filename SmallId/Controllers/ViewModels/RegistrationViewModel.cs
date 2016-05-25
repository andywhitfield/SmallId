namespace SmallId.Controllers.ViewModels
{
    public class RegistrationViewModel
    {
        public string EmailAddress { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public bool RememberMe { get; set; }
    }
}