// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.TwitterSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// Helper class for video and GIF entities
    /// </summary>
    public class ExtendedMediaTwitter
    {
        /// <summary>
        /// ID of the media
        /// </summary>
        [JsonProperty("id")]
         public long Id { get; set; }
        
        /// <summary>
        ///  URL for media
        /// </summary>
        [JsonProperty("media_url_https")]
        public string MediaUrlHttps { get; set; }

        /// <summary>
        /// Type of media i.e. Photo, Video, GIF
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Only in case of GIF and Videos
        /// </summary>
        [JsonProperty("video_info")]
        public VideoInfoTwitter VideoInfo { get; set; }

        public string Content { get; set; }
    }
}