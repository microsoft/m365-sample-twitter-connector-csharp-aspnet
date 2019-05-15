// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.TwitterSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// Helper class for different Variants of Video type Media
    /// </summary>
    public class VideoVariantTwitter
    {
        /// <summary>
        /// Bitrate of Video and in case of GIF it's 0
        /// </summary>
        [JsonProperty("bitrate")]
        public int Bitrate { get; set; }

        /// <summary>
        /// Video Content type
        /// </summary>
        [JsonProperty("content_type")]
        public string ContentType { get; set; }

        /// <summary>
        /// Video download URL
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}