// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.TwitterSDK
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Sample.Connector;

    /// <summary>
    /// Transform Twitter Data Model to Item Schema
    /// </summary>
    public class TwitterSchemaToItemMapper
    {
        #region constructor
        
        /// <summary>
        /// Date template
        /// </summary>
        private const string Const_TwitterDateTemplate = "ddd MMM dd HH:mm:ss +ffff yyyy";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="taskInfo">The Task Info</param>
        public TwitterSchemaToItemMapper()
        {
        }

        #endregion constructor

        #region public methods

        /// <summary>
        /// Add an Twitter tweet to Email
        /// </summary>
        /// <param name="tweet">Twitter Tweet</param>
        public async Task<List<Item>> MapTweetToItemList(Tweet tweet)
        {
            List<Item> listItems = new List<Item>();
            string ParentId = string.Empty;
            string ThreadId = string.Empty;
            if (tweet.IsQuotedStatus && tweet.QuotedStatus != null)
            {
                listItems.AddRange(await MapTweetToItemList(tweet.QuotedStatus));
                ParentId = tweet.QuotedStatus.Tweetid;
            }

            if(tweet.Retweeted && tweet.RetweetedStatus != null)
            {
                listItems.AddRange(await MapTweetToItemList(tweet.RetweetedStatus));
                ParentId = tweet.RetweetedStatus.Tweetid;
            }

            Item postItem = MapTweetToItem(tweet); 
            postItem.ParentId = ParentId;
            postItem.ThreadId = ThreadId;
            listItems.Add(postItem);

            return listItems;
        }

        private Item MapTweetToItem(Tweet tweet)
        {
            Item postItem = new Item()
            {
                SchemaVersion = new Version(1, 0),
                Id = tweet.Tweetid,
                ContainerId = tweet.User.Id.ToString(),
                ContainerName = tweet.User.Name,
                SourceType = "Twitter",
                ItemType = "Tweet",
                ContentType = ContentType.Text,
                Content = tweet.TweetText.ToString(),
                ParentId = tweet.InReplyToStatusId,
                ThreadId = tweet.InReplyToStatusId,
                SentTimeUtc = DateTime.ParseExact(tweet.CreatedAt, Const_TwitterDateTemplate, new System.Globalization.CultureInfo("en-US")),
                Sender = TweetUserToItemUser(tweet.User),
                Recipients = Array.Empty<User>(),
                NumOfLikes = tweet.FavoriteCount,
                MessagePreviewText = tweet.TweetText.ToString()
            };

            postItem.ContentAttachments = MapAttachments(tweet);
            return postItem;
        }
        #endregion public methods

        private static User TweetUserToItemUser(UserTwitter user)
        {
            return new User
            {
                Id = user?.Id.ToString(),
                UserProfilePhotoUrl = user.ProfileImageUrl,
                Name = user?.Name ?? "Anonymous",
                EmailAddress = user?.Id.ToString() ?? "Anonymous"
            };
        }

        private List<ContentAttachment> MapAttachments(Tweet tweet)
        {
            List<ContentAttachment> attachments = null;
            if (!String.IsNullOrEmpty(tweet.Entities.MediaObjects?.ToList().FirstOrDefault()?.MediaUrlHttps))
            {
                foreach (var mediaObjects in tweet.Entities.MediaObjects.ToList())
                {
                    if (attachments == null)
                    {
                        attachments = new List<ContentAttachment>();
                    }

                    ContentAttachment attachment = new ContentAttachment()
                    {
                        AttachmentType = "media",
                        Content = mediaObjects.Content,
                        Uri = new Uri(mediaObjects.MediaUrlHttps),
                    };
                    attachments.Add(attachment);
                }

                foreach (var mediaObjects in tweet.ExtendedEntities.ExtendedMediaObjects.ToList())
                {
                    if (!attachments.Where(attachment => attachment.Uri.ToString().Equals(mediaObjects.MediaUrlHttps)).Any())
                    {
                        ContentAttachment attachment = new ContentAttachment()
                        {
                            AttachmentType = "media",
                            Content = mediaObjects.Content,
                            Uri = new Uri(mediaObjects.MediaUrlHttps),
                        };
                        attachments.Add(attachment);
                    }
                }
            }
            return attachments;
        }
    }    
}

