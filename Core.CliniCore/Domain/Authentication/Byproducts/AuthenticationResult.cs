using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Users;

namespace Core.CliniCore.Domain.Authentication.Byproducts
{
    public class AuthenticationResult
    {
        public bool Success { get; init; }
        public IUserProfile? Profile { get; init; }
        public AuthenticationFailureReason FailureReason { get; init; }

        private AuthenticationResult() { }

        public static AuthenticationResult Ok(IUserProfile profile) => new()
        {
            Success = true,
            Profile = profile,
            FailureReason = AuthenticationFailureReason.None
        };

        public static AuthenticationResult Fail(AuthenticationFailureReason reason) => new()
        {
            Success = false,
            Profile = null,
            FailureReason = reason
        };
    }
}
