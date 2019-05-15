// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.TwitterConnector
{
    using System;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.Owin;
    using Sample.Connector;
    using Sample.TwitterSDK;

    /// <summary>
    /// Helper class for Twitter OAuth handling page.
    /// </summary>
    public class TwitterOAuthHelper
    {
        /// <summary>
        /// Cookie key for nonce token
        /// </summary>
        private const string TwitterOAuthValidationToken = "Twitter-Oauth-Validation-Token";
        
        /// <summary>
        /// Authenticates Twitter login, gets access codes for pages and redirects user to Import page
        /// </summary>
        /// <param name="code">temporary Twitter access code</param>
        /// <param name="url">Current request Url</param>
        /// <returns>Redirect to Import page with authentication acknowledgment</returns>
        public static Task<bool> StoreToken(IOwinContext context, string code, Uri url)
        {
            if (string.IsNullOrEmpty(code))
            {
                return Task.FromResult<bool>(false);
            }

            RequestCookieCollection cookies = context.Request.Cookies;
            string jobId = cookies["jobId"];

            string redirectUrl = url.GetLeftPart(UriPartial.Path);

            return Task.Run(() => StoreTokenHelper("Twitter", code, redirectUrl, jobId));
        }

        public static async Task<bool> StoreTokenHelper(string jobType, string code, string redirectUrl, string jobId)
        {
            var azureTableProvider = new AzureTableProvider(Settings.StorageAccountConnectionString);
            var client = new RestApiRepository(SettingsTwitter.TwitterAuthEndPoint);
            IConnectorSourceProvider sourceProvider = new TwitterProvider(azureTableProvider, client, new TwitterAuthProvider(client, azureTableProvider));
            await sourceProvider.StoreOAuthToken(code, redirectUrl, jobId);

            return true;
        }

        /// <summary>
        /// Redirect user to import page with current job opened
        /// </summary>
        /// <param name="context">Current OWin Context</param>
        /// <param name="state">State param</param>
        /// <returns>Redirects to specified page</returns>
        public static bool IsAuthenticRequest(IOwinContext context, string state)
        {
            return true;
        }

        /// <summary>
        /// Redirect user to import page with current job opened
        /// </summary>
        /// <param name="context">Current OWin Context</param>
        /// <param name="encodedUrl">Encoded Url to be redirected </param>
        /// <returns>Redirects to specified page</returns>
        public static string GetOAuthUrl(IOwinContext context, string encodedUrl)
        {
            string decodedUrl = HttpUtility.UrlDecode(encodedUrl);
            string stateToken = CreateAndSetStateCookie(context);
            return string.Format("{0}&state={1}", decodedUrl, stateToken);
        }

        /// <summary>
        /// Create a nonce token and store in cookie
        /// </summary>
        /// <param name="context">Current OWin Context</param>
        /// <returns>Generate nonce token</returns>
        private static string CreateAndSetStateCookie(IOwinContext context)
        {
            Guid g = Guid.NewGuid();
            string guidString = Convert.ToBase64String(g.ToByteArray());
            guidString = HttpUtility.UrlEncode(guidString);
            context.Response.Cookies.Append(
                TwitterOAuthValidationToken,
                guidString,
                new CookieOptions() { Path = context.Request.PathBase.Value, Secure = context.Request.IsSecure, HttpOnly = true });
            return guidString;
        }
    }
}