// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.TwitterSDK
{
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;
    using Sample.Connector;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Sample.TwitterSDK.Auth;

    public class JobProcessorTwitter : JobProcessorBase
    {
        /// <summary>
        /// base url for Twitter api
        /// </summary>
        private readonly string url;

        /// <summary>
        /// AppID for making requests
        /// </summary>
        private readonly string AppID;

        /// <summary>
        /// AppToken value
        /// </summary>
        private readonly string AppSecret;

        /// <summary>
        /// For Building Emails
        /// </summary>
        private TwitterSchemaToItemMapper twitterItemMapper;

        /// <summary>
        /// For downloading content
        /// </summary>
        private IDownloader downloader;

        /// <summary>
        /// Azure Table instance provider;
        /// </summary>
        private AzureTableProvider azureTableProviderInstance = new AzureTableProvider(Settings.StorageAccountConnectionString);

        /// <summary>
        /// Azure Table instance provider;
        /// </summary>
        private CloudTable PageJobMappingTable;

        /// <summary>
        /// Date template
        /// </summary>
        private const string Const_TwitterDateTemplate = "ddd MMM dd HH:mm:ss +ffff yyyy";

        private IUploader uploader;

        /// <summary>
        /// Constructor
        /// </summary>
        public JobProcessorTwitter(IDownloader downloader, IUploader uploader, TwitterSchemaToItemMapper twitterItemMapper)
        {
            url = SettingsTwitter.TwitterEndPoint+ "/1.1/statuses/user_timeline.json";
            AppID = SettingsTwitter.TwitterAppId;
            AppSecret = SettingsTwitter.TwitterAppSecret;
            this.twitterItemMapper = twitterItemMapper;
            this.downloader = downloader;
            this.uploader = uploader;
        }

        /// <summary>
        /// Fetches Data wrt given Tweet ID
        /// </summary>
        /// <param name="taskInfo">contains the TweetID for which data is to be fetched</param>
        public override async Task<List<ItemMetadata>> FetchData(ConnectorTask taskInfo, string sourceInfo)
        {
            Trace.TraceInformation("Data fetch Started");
            List<ItemMetadata> itemMetaData = new List<ItemMetadata>();
            SourceInfoTwitter twitterSourceInfo = JsonConvert.DeserializeObject<SourceInfoTwitter>(sourceInfo);
            OAuth1Token token = new OAuth1Token(SettingsTwitter.TwitterAppId, SettingsTwitter.TwitterAppSecret, twitterSourceInfo.ClientToken, twitterSourceInfo.ClientSecret);
            var filterTime = taskInfo.EndTime;
            OAuth1Helper oAuth1Helper = new OAuth1Helper(url, token, HttpMethod.Get.ToString().ToUpperInvariant());
            while (true)
            {
                Dictionary<string, string> param = getParams(taskInfo, twitterSourceInfo);

                string queryString = oAuth1Helper.GetQueryString(param);
                string authHeader = oAuth1Helper.GenerateAuthorizationHeader();
                AuthenticationHeaderValue header = new AuthenticationHeaderValue("OAuth", authHeader);
                List<Tweet> tweets = await downloader.GetWebContent<List<Tweet>, ErrorsTwitter>(queryString, header);
                bool isScheduleCompleted = false;
                if (tweets != null && tweets.Any())
                {
                    var minId = tweets.Select(t => long.Parse(t.Tweetid)).ToList<long>().Min().ToString() ?? twitterSourceInfo.SinceId;
                    isScheduleCompleted = DateTime.Compare(DateTime.ParseExact(tweets.Where(t => t.Tweetid.Equals(minId)).First().CreatedAt, Const_TwitterDateTemplate, new System.Globalization.CultureInfo("en-US")), taskInfo.EndTime) > 0;
                }

                if (tweets == null || tweets.Count == 0 || isScheduleCompleted)
                    break; // When no new data to get since sinceID(last fetched tweet)
                
                twitterSourceInfo.SinceId = tweets.Select(t => long.Parse(t.Tweetid)).ToList<long>().Max().ToString();
                tweets.RemoveAll(t => DateTime.Compare(DateTime.ParseExact(t.CreatedAt, Const_TwitterDateTemplate, new System.Globalization.CultureInfo("en-US")), taskInfo.StartTime) < 0);
                tweets.RemoveAll(t => DateTime.Compare(DateTime.ParseExact(t.CreatedAt, Const_TwitterDateTemplate, new System.Globalization.CultureInfo("en-US")), taskInfo.EndTime) > 0);

                Trace.TraceInformation($"Tweets Fetched {tweets.Count}");              
                
                if(tweets.Any())
                {
                    foreach (var tweet in tweets)
                    {
                        var enrichedTweet = await EnrichTweetWithAttachments(tweet);
                        itemMetaData.AddRange(await UploadTweet(twitterItemMapper, enrichedTweet, taskInfo));
                    }
                    twitterSourceInfo.SinceId = tweets.Select(t => long.Parse(t.Tweetid)).ToList<long>().Max().ToString();
                }
            }

            return itemMetaData;
        }

        public async Task UpdateSourceInfo(ConnectorTask taskInfo, SourceInfoTwitter twitterSourceInfo)
        {
            PageJobMappingTable = azureTableProviderInstance.GetAzureTableReference(Settings.PageJobMappingTableName);
            Expression<Func<PageJobEntity, bool>> filter = (entity => entity.RowKey == taskInfo.JobId);
            CloudTable pageJobMappingTable = azureTableProviderInstance.GetAzureTableReference(Settings.PageJobMappingTableName);
            List<PageJobEntity> pageJobEntityList = await azureTableProviderInstance.QueryEntitiesAsync<PageJobEntity>(pageJobMappingTable, filter);
            PageJobEntity pageJobEntity = pageJobEntityList[0];
            pageJobEntity.SourceInfo = JsonConvert.SerializeObject(twitterSourceInfo);
            await azureTableProviderInstance.InsertOrReplaceEntityAsync(PageJobMappingTable, pageJobEntity);
        }

        /// <summary>
        /// Handles Twitter Tweet
        /// </summary>
        /// <param name="twitterItemMapper">Transforms Twitter data to Item Schema</param>
        /// <param name="tweet">Twitter tweet</param>
        private async Task<List<ItemMetadata>> UploadTweet(TwitterSchemaToItemMapper twitterItemMapper, Tweet tweet, ConnectorTask taskInfo)
        {
            List<Item> postItem = await twitterItemMapper.MapTweetToItemList(tweet);
            List<ItemMetadata> itemMetaDataList = new List<ItemMetadata>();
            foreach (var item in postItem)
            {
                string fileName = await uploader.UploadItem(taskInfo.JobId, taskInfo.TaskId, item);
                Trace.TraceInformation("Tweet Uploaded to Azure Blobs");
                itemMetaDataList.Add(new ItemMetadata(item.Id, item.SentTimeUtc, fileName));
            }

            return itemMetaDataList;
        }

        private async Task<Tweet> EnrichTweetWithAttachments(Tweet tweet)
        {
            if(tweet.IsQuotedStatus && tweet.QuotedStatus != null)
            {
                tweet.QuotedStatus = await EnrichTweetWithAttachments(tweet.QuotedStatus);
            }

            if (tweet.Retweeted && tweet.RetweetedStatus != null)
            {
                tweet.RetweetedStatus = await EnrichTweetWithAttachments(tweet.RetweetedStatus);
            }

            if (tweet.Entities != null && tweet.Entities.MediaObjects != null)
            {
                List<DefaultMediaTwitter> entitiesMediaObject = null;
                foreach (var media in tweet.Entities.MediaObjects)
                {
                    if (entitiesMediaObject == null)
                    {
                        entitiesMediaObject = new List<DefaultMediaTwitter>();
                    }
                    media.Content = await downloader.DownloadFileAsBase64EncodedString(media.MediaUrlHttps);
                    entitiesMediaObject.Add(media);
                }
                tweet.Entities.MediaObjects = entitiesMediaObject;
            }

            if (tweet.ExtendedEntities != null && tweet.ExtendedEntities.ExtendedMediaObjects != null)
            {
                List<ExtendedMediaTwitter> extEntitiesMediaObject = null;
                foreach (var media in tweet.ExtendedEntities.ExtendedMediaObjects)
                {
                    if (extEntitiesMediaObject == null)
                    {
                        extEntitiesMediaObject = new List<ExtendedMediaTwitter>();
                    }
                    media.Content = await downloader.DownloadFileAsBase64EncodedString(media.MediaUrlHttps);
                    extEntitiesMediaObject.Add(media);
                }
                tweet.ExtendedEntities.ExtendedMediaObjects = extEntitiesMediaObject;
            }
            return tweet;
        }

        /// <summary>
        /// Set parameters accordingly
        /// </summary>
        /// <param name="sourceInfo"></param>
        /// <returns>parameters list</returns>
        private Dictionary<string, string> getParams(ConnectorTask taskInfo, SourceInfoTwitter sourceInfo)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                {"include_entities", "true"},
                {"count", "200"},
                {"include_rts", "true"},
            };

            if (Convert.ToInt64(sourceInfo.SinceId) > 0)
                parameters.Add("since_id", sourceInfo.SinceId);
            return parameters;
        }
    }
}
