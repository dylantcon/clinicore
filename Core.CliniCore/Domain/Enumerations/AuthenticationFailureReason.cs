namespace Core.CliniCore.Domain.Enumerations
{
    public enum AuthenticationFailureReason
    {
        None,
        InvalidCredentials  // Covers: user not found OR wrong password (obfuscated)
    }
}
