// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.TwitterSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// Helper class for mentioned users entity in the tweet or retweets
    /// </summary>
    public class UserMentionTwitter
    {
        /// <summary>
        /// Twitter handle of user
        /// </summary>
        [JsonProperty("screen_name")]
        public string ScreenName { get; set; }

        /// <summary>
        /// Name of the user
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// ID of the user
        /// </summary>
        [JsonProperty("id")]
        public long Id { get; set; }


    }
}