// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.TwitterSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// Helper Class for hashtag entity
    /// </summary>
    public class HashtagEntityTwitter
    {
        /// <summary>
        /// value of hashtag
        /// </summary>
        [JsonProperty("text")]
        public string HashtagText { get; set; }

    }
}