// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

//Twitter API Doc for creating Authentication headers : "https://developer.twitter.com/en/docs/basics/authentication/guides/authorizing-a-request"

namespace Sample.TwitterSDK.Auth
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Net;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Cryptography;

    /// <summary>
    /// Helper class for Oauth1 for Twitter
    /// </summary>
    public class OAuth1Helper
    {
        /// <summary>
        /// Request URI
        /// </summary>
        private Uri RequestUri { get; set; }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        private Dictionary<string, object> RequestParameters { get; set; }

        /// <summary>
        /// Gets or sets the oauth tokens.
        /// </summary>
        /// <value>The tokens.</value>
        private OAuth1Token Token { get; }

        private string httpMethod { get; }

        /// <summary>
        /// OAuth Parameters key names to include in the Authorization header.
        /// </summary>
        private readonly string[] OAuthParametersToIncludeInHeader = {
            "oauth_version",
            "oauth_nonce",
            "oauth_timestamp",
            "oauth_signature_method",
            "oauth_consumer_key",
            "oauth_token",
            "oauth_verifier"
        };

        /// <summary>
        /// Parameters that may appear in the list, but should never be included in the header or the request.
        /// </summary>
        private readonly string[] SecretParameters = {
            "oauth_consumer_secret",
            "oauth_token_secret",
            "oauth_signature"
        };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="url">URL to API call</param>
        /// <param name="token">OAuth1 token value</param>
        public OAuth1Helper(string url,OAuth1Token token, string httpMethod)
        {
            this.RequestUri = new Uri(url);
            this.Token = token;
            this.httpMethod = httpMethod;
        }
        /// <summary>
        /// Adds the parameters to request uri.
        /// </summary>
        public string GetQueryString(Dictionary<string, string> parametersToAppend)
        {
            SetupOAuthParameters();
            AppendAdditionalParams(parametersToAppend);
            AppendSignature(this.RequestUri.AbsoluteUri);
            StringBuilder requestParametersBuilder = new StringBuilder(this.RequestUri.AbsoluteUri);
            requestParametersBuilder.Append(this.RequestUri.Query.Length == 0 ? "?" : "&");

            Dictionary<string, object> fieldsToInclude = new Dictionary<string, object>(RequestParameters.Where(p => !OAuthParametersToIncludeInHeader.Contains(p.Key) &&
                                                                                                                   !SecretParameters.Contains(p.Key)).ToDictionary(p => p.Key, p => p.Value));
            foreach (KeyValuePair<string, object> item in fieldsToInclude)
            {
                if (item.Value is string)
                    requestParametersBuilder.Append($"{item.Key}={WebUtility.UrlEncode((string)item.Value)}&");
            }
            requestParametersBuilder.Remove(requestParametersBuilder.Length - 1, 1);

            return requestParametersBuilder.ToString();
        }

        private void AppendSignature(string baseString2)
        {
            RequestParameters.Add("oauth_signature", GenerateSignature());
        }

        private void AppendAdditionalParams(Dictionary<string, string> parametersToAppend)
        {
            if (parametersToAppend != null && parametersToAppend.Any())
            {
                foreach (var field in parametersToAppend)
                {
                    RequestParameters.Add(field.Key, field.Value);
                }
            }
        }


        /// <summary>
        /// Sets up the OAuth request details.
        /// </summary>
        private void SetupOAuthParameters()
        {
            RequestParameters = new Dictionary<string, object>
            {
                {"oauth_version", "1.0"},
                {"oauth_nonce", new Random().Next(int.MaxValue).ToString()},
                {"oauth_timestamp", DateTimeOffset.UtcNow.ToUniversalTime().ToUnixTimeSeconds().ToString()},
                {"oauth_signature_method", "HMAC-SHA1"},
                {"oauth_consumer_key", Token.AppID},
                {"oauth_consumer_secret", Token.AppSecret},
                {"oauth_token", Token.ClientToken},
                {"oauth_token_secret", Token.ClientSecret},
            };
        }

        /// <summary>
        /// Generates signature as per twitter needs by doing hashing with HMAC-SHA1
        /// </summary>
        /// <returns>signature string</returns>
        private string GenerateSignature()
        {
            IEnumerable<KeyValuePair<string, object>> nonSecretParameters;

            nonSecretParameters = (from p in RequestParameters
                                   where (!SecretParameters.Contains(p.Key))
                                   select p);

            Uri urlForSigning = this.RequestUri;

            string signatureBaseString =
                $"{this.httpMethod}&{WebUtility.UrlEncode((urlForSigning.ToString()))}&{UrlEncode(nonSecretParameters)}";

            string key = $"{WebUtility.UrlEncode(Token.AppSecret)}&{WebUtility.UrlEncode(Token.ClientSecret)}";

            // Generate the hash
            HMACSHA1 hmacsha1 = new HMACSHA1(Encoding.UTF8.GetBytes(key));
            byte[] signatureBytes = hmacsha1.ComputeHash(Encoding.UTF8.GetBytes(signatureBaseString));
            return Convert.ToBase64String(signatureBytes);
        }

        /// <summary>
        /// Generates the authorization header.
        /// </summary>
        /// <returns>The string value of the HTTP header to be included for OAuth requests.</returns>
        public string GenerateAuthorizationHeader()
        {
            StringBuilder authHeaderBuilder = new StringBuilder();

            var sortedParameters = from p in RequestParameters
                                   where OAuthParametersToIncludeInHeader.Contains(p.Key)
                                   orderby p.Key, WebUtility.UrlEncode((p.Value is string) ? (string)p.Value : string.Empty)
                                   select p;

            foreach (var item in sortedParameters)
            {
                authHeaderBuilder.Append($",{WebUtility.UrlEncode(item.Key)}=\"{WebUtility.UrlEncode(item.Value as string)}\"");
            }

            authHeaderBuilder.Append($",oauth_signature=\"{WebUtility.UrlEncode(RequestParameters["oauth_signature"] as string)}\"");

            return authHeaderBuilder.ToString().Substring(1);
        }

        /// <summary>
        /// Encodes a series of key/value pairs for inclusion in a URL querystring.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>A string of all the <paramref name="parameters"/> keys and value pairs with the values encoded.</returns>
		private string UrlEncode(IEnumerable<KeyValuePair<string, object>> parameters)
        {
            StringBuilder parameterString = new StringBuilder();

            var paramsSorted = from p in parameters
                               orderby p.Key, p.Value
                               select p;

            foreach (var item in paramsSorted)
            {
                if (item.Value is string)
                {
                    if (parameterString.Length > 0)
                    {
                        parameterString.Append("&");
                    }

                    parameterString.Append($"{item.Key}={(string) item.Value}");
                }
            }
            return WebUtility.UrlEncode(parameterString.ToString());
        }
    }
}