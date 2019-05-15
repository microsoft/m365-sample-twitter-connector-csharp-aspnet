// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.TwitterSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// Helper class for Twitter Error response
    /// </summary>
    public class ErrorTypeTwitter
    {
        /// <summary>
        /// Error messaege
        /// </summary>
        [JsonProperty("message")]
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// Error Code
        /// </summary>
        [JsonProperty("code")]
        public int Code { get; set; }
    }
}