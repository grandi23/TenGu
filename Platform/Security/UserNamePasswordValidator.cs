
namespace Platform.Security
{
    public class UserNamePasswordValidator : System.IdentityModel.Selectors.UserNamePasswordValidator
    {
        public override void Validate(string userName, string password)
        {
            // Nothing to do.
        }
    }
}
