// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.TwitterSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// Helper class for Media object in tweet, Even in the case of video and GIF they are displayed as still image on the status.
    /// </summary>
    public class DefaultMediaTwitter
    {
        /// <summary>
        /// ID of the media
        /// </summary>
        [JsonProperty("id")]
        public long Id { get; set; }
        
        /// <summary>
        /// Download URL for default media
        /// </summary>
        [JsonProperty("media_url_https")]
        public string MediaUrlHttps { get; set; }

        public string Content { get; set; }
    }
}