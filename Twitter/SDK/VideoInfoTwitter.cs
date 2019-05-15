// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.TwitterSDK
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Helper class for information about video and GIF media
    /// </summary>
    public class VideoInfoTwitter
    {
        /// <summary>
        /// Aspect ratio of the video i.e for 175 x 131 it would be [ 175, 131]
        /// </summary>
        [JsonProperty("aspect_ratio")]
        public IList<int> AspectRatio { get; set; }
        
        /// <summary>
        /// Field only in case of video and it's video length in miliseconds
        /// </summary>
        [JsonProperty("duration_millis")]
        public int DurationInMillis { get; set; }
        
        /// <summary>
        /// For different varients of video, like 480p, 720p etc..
        /// </summary>
        [JsonProperty("variants")]
        public IList<VideoVariantTwitter> VideoVariants { get; set; }
    }
}