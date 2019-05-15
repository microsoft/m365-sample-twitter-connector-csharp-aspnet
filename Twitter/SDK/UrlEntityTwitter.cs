// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.TwitterSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// URL details inside of tweet
    /// </summary>
    public class UrlEntityTwitter
    {
        /// <summary>
        /// Twitter compressed URL
        /// </summary>
        [JsonProperty("url")]
        public string ShortUrl { get; set; }

        /// <summary>
        /// Original URL
        /// </summary>
        [JsonProperty("expanded_url")]
        public string ExpandedUrl { get; set; }


    }
}