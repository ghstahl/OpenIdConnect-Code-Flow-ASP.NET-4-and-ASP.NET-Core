namespace Microsoft.Owin.Security.OpenIdConnect
{
    /// <summary>
    /// Name/Value representing an token.
    /// </summary>
    public class AuthenticationToken
    {
        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Value.
        /// </summary>
        public string Value { get; set; }
    }
}