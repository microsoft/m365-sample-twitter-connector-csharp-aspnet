// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.TwitterSDK
{
    using Newtonsoft.Json;

    public class Tweet
    {
        /// <summary>
        ///  Time of creation Ex : "created_at": "Thu Apr 06 15:24:15 +0000 2017",
        /// </summary>
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        /// <summary>
        /// Tweet Global ID 
        /// </summary>
        [JsonProperty("id_str")]
        public string Tweetid { get; set; }

        /// <summary>
        /// Tweet or Retweet text of authenticating user. 
        /// </summary>
        [JsonProperty("full_text")]
        public string TweetText { get; set; }

        /// <summary>
        /// Tells if tweet is truncated or not
        /// </summary>
        [JsonProperty("truncated")]
        public bool Truncated { get; set; }

        /// <summary>
        /// User details of owner of tweet
        /// </summary>
        [JsonProperty("user")]
        public UserTwitter User { get; set; }

        /// <summary>
        /// Objects within tweets like Hashtags, URLs, single Media, User mentions ...
        /// </summary>
        [JsonProperty("entities")]
        public EntitiesTwitter Entities { get; set; }

        /// <summary>
        /// For photoes, videos, GIFs etc.. media entities
        /// </summary>
        [JsonProperty("extended_entities")]
        public ExtendedEntitiesTwitter ExtendedEntities { get; set; }

        /// <summary>
        /// Source from which the tweet was sent Ex : "source":"Twitter for Android"
        /// </summary>
        [JsonProperty("source")]
        public string Source { get; set; }

        /// <summary>
        /// If tweet was retweeted details of original tweet else it's null
        /// </summary>
        [JsonProperty("retweeted_status")]
        public Tweet RetweetedStatus { get; set; }

        /// <summary>
        /// Retweet count
        /// </summary>
        [JsonProperty("retweet_count")]
        public int RetweetCount { get; set; }

        /// <summary>
        /// Is a Retweet
        /// </summary>
        [JsonProperty("retweeted")]
        public bool Retweeted { get; set; }

        /// <summary>
        /// Favourite count i.e. Likes on tweet.
        /// </summary>
        [JsonProperty("favorite_count")]
        public int FavoriteCount { get; set; }

        /// <summary>
        /// Indicates approximately how many times this Tweet has been quoted by Twitter users.
        /// </summary>
        [JsonProperty("quote_count")]
        public int? QuoteCount { get; set; }

        /// <summary>
        /// Id of the status in whose reply this tweet is.
        /// </summary>
        [JsonProperty("in_reply_to_status_id_str")]
        public string InReplyToStatusId { get; set; }

        /// <summary>
        /// User ID of the user in whose reply this tweet is.
        /// </summary>
        [JsonProperty("in_reply_to_user_id")]
        public long? InReplyToUserId { get; set; }

        /// <summary>
        /// Screen name of the user in whose reply this tweet is.
        /// </summary>
        [JsonProperty("in_reply_to_screen_name")]
        public string InReplyToScreenName { get; set; }

        /// <summary>
        /// Id of the status which is quotes in this tweet.
        /// </summary>
        [JsonProperty("is_quote_status")]
        public bool IsQuotedStatus { get; set; }

        /// <summary>
        /// Id of the status which is quotes in this tweet.
        /// </summary>
        [JsonProperty("quoted_status_id_str")]
        public string QuotedStatusId { get; set; }

        /// <summary>
        /// Quoted status.
        /// </summary>
        [JsonProperty("quoted_status")]
        public Tweet QuotedStatus { get; set; }
    }
}
