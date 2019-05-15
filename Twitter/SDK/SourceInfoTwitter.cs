// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.TwitterSDK
{
    public class SourceInfoTwitter
    {
        /// <summary>
        /// Client Oauth Token
        /// </summary>
        public string ClientToken { get; set; }

        /// <summary>
        /// Client Oauth Secret
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// ID of last tweet that is used to fetch tweets after this tweet
        /// </summary>
        public string SinceId { get; set; }
    }
}
