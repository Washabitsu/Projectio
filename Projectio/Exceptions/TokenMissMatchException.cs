namespace Projectio.Exceptions
{
    public class TokenMissMatchException : Exception
    {
        /* Since I trust the token to be valid if it was signed with the correct private key in the case
         * that the token contains a username that does not exist in the database I will throw this exception.
         * This could be a IOC that the attacker is trying to use a token that was signed with the correct private key
         * To impersonate a user that does not exist in the database or even enumerate users.
        */
        public TokenMissMatchException() : base("The provided token does not match the expected token.")
        { }
    }
}
