// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.TwitterSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// Helper class for User data in twitter
    /// </summary>
    public class UserTwitter
    {
        /// <summary>
        /// User's twitter ID
        /// </summary>
        [JsonProperty("id")]
        public long Id { get; set; }

        /// <summary>
        /// User's Full Name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// User's twitter display name
        /// </summary>
        [JsonProperty("screen_name")]
        public string ScreenName { get; set; }

        /// <summary>
        /// User's location in profile
        /// </summary>
        [JsonProperty("location")]
        public string Location { get; set; }

        /// <summary>
        /// User's Description in profile
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// URL mentioned in profile
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// Entities in the profile
        /// </summary>
        [JsonProperty("entities")]
        public EntitiesTwitter Entities { get; set; }

        /// <summary>
        /// Number of total tweets
        /// </summary>
        [JsonProperty("statuses_count")]
        public long TotalTweets { get; set; }

        /// <summary>
        /// Number of followers
        /// </summary>
        [JsonProperty("followers_count")]
        public long FollowersCount { get; set; }

        /// <summary>
        /// Number of followings
        /// </summary>
        [JsonProperty("friends_count")]
        public long FriendsCount { get; set; }

        /// <summary>
        /// Number of likes on the other's posts
        /// </summary>
        [JsonProperty("favourites_count")]
        public long FavouritesCount { get; set; }

        /// <summary>
        /// Url for profile picture
        /// </summary>
        [JsonProperty("profile_image_url_https")]
        public string ProfileImageUrl { get; set; }

    }
}