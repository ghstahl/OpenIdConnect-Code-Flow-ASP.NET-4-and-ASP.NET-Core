using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security.OpenIdConnect
{
    /// <summary>
    /// Extension methods for storing authentication tokens in <see cref="AuthenticationProperties"/>.
    /// </summary>
    public static class AuthenticationTokenExtensions
    {
        private static string TokenNamesKey = ".TokenNames";
        private static string TokenKeyPrefix = ".Token.";

        /// <summary>
        /// Stores a set of authentication tokens, after removing any old tokens.
        /// </summary>
        /// <param name="properties">The <see cref="AuthenticationProperties"/> properties.</param>
        /// <param name="tokens">The tokens to store.</param>
        public static void StoreTokens(this AuthenticationProperties properties, IEnumerable<AuthenticationToken> tokens)
        {

            foreach (var token in tokens)
            {
                properties.Dictionary.Add(token.Name,token.Value);
            }
        }

        /// <summary>
        /// Returns the value of a token.
        /// </summary>
        /// <param name="properties">The <see cref="AuthenticationProperties"/> properties.</param>
        /// <param name="tokenName">The token name.</param>
        /// <returns>The token value.</returns>
        public static string GetTokenValue(this AuthenticationProperties properties, string tokenName)
        {
            if (!properties.Dictionary.ContainsKey(tokenName))
            {
                return null;
            }

            return properties.Dictionary[tokenName];
        }


        /// <summary>
        /// Returns all of the AuthenticationTokens contained in the properties.
        /// </summary>
        /// <param name="properties">The <see cref="AuthenticationProperties"/> properties.</param>
        /// <returns>The authentication toekns.</returns>
        public static IEnumerable<AuthenticationToken> GetTokens(this AuthenticationProperties properties)
        {
            var q = from item in properties.Dictionary
                let c = new AuthenticationToken()
                {
                    Name = item.Key,
                    Value = item.Value
                }
                select c;
            return q.ToList();
        }

    }
}