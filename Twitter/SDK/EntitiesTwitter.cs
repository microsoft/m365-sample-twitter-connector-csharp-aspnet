// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.TwitterSDK
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Entities within tweets, retweets, user
    /// </summary>
    public class EntitiesTwitter
    {
        /// <summary>
        /// Hashtags inside the tweet status
        /// </summary>
        [JsonProperty("hashtags")]
        public List<HashtagEntityTwitter> Hashtags { get; set; }

        /// <summary>
        /// Mentioned users in tweet status
        /// </summary>
        [JsonProperty("user_mentions")]
        public List<UserMentionTwitter> UserMentions { get; set; }

        /// <summary>
        /// URLs inside tweet status
        /// </summary>
        [JsonProperty("urls")]
        public List<UrlEntityTwitter> Urls { get; set; }

        /// <summary>
        /// Media photo, video, GIF etc..   
        /// </summary>
        [JsonProperty("media")]
        public List<DefaultMediaTwitter> MediaObjects { get; set; }
    }
}
