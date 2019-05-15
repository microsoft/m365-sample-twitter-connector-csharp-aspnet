// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.TwitterSDK.Auth
{
    /// <summary>
    /// Helper class for Oauth Token values
    /// </summary>
    public class OAuth1Token
    {
        /// <summary>
        /// Application ID
        /// </summary>
        public string AppID { get; }
        
        /// <summary>
        /// Application secret 
        /// </summary>
        public string AppSecret { get; }

        /// <summary>
        /// Client token set from TwitterSourceInfo
        /// </summary>
        public string ClientToken { get; }

        /// <summary>
        /// ClientSecret set from TwitterSourceInfo
        /// </summary>
        public string ClientSecret { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="AppID"></param>
        /// <param name="AppSecret"></param>
        /// <param name="ClientToken"></param>
        /// <param name="ClienSecret"></param>
        public OAuth1Token(string AppID, string AppSecret, string ClientToken, string ClienSecret)
        {
            this.AppID = AppID;
            this.AppSecret = AppSecret;
            this.ClientSecret = ClienSecret;
            this.ClientToken = ClientToken;
        }
    }
}
