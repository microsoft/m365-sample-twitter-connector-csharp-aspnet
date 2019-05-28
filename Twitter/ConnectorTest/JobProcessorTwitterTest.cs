using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Sample.Connector;
using Sample.TwitterSDK;

namespace ConnectorTest
{
    [TestClass]
    public class JobProcessorTwitterTest
    {
        private JobProcessorTwitter jobProcessor;
        private Mock<IDownloader> downloader;
        private Mock<IUploader> uploader;
        private MockRepository mockRepo;

        [TestInitialize]
        public void TestInitialize()
        {
            mockRepo = new MockRepository(MockBehavior.Default) { DefaultValue = DefaultValue.Mock };
            downloader = mockRepo.Create<IDownloader>();
            uploader = mockRepo.Create<IUploader>();
            downloader.Setup(x => x.DownloadFileAsBase64EncodedString(It.IsAny<string>())).ReturnsAsync(string.Empty);
            uploader.Setup(x => x.UploadItem(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Item>())).ReturnsAsync("filename");
            jobProcessor = new JobProcessorTwitter(downloader.Object, uploader.Object, new TwitterSchemaToItemMapper());
        }
        
        [TestMethod]
        public async Task FetchData_WhenSinceIdisZero_ThenDataFetched()
        {
            var tweets = JsonConvert.DeserializeObject<List<Tweet>>(File.ReadAllText(@"FakeTweets.json"));
            downloader.Setup(x => x.GetWebContent<List<Tweet>, ErrorsTwitter>(It.Is<string>( s => s == "https://api.twitter.com/1.1/statuses/user_timeline.json?include_entities=true&count=200&include_rts=true&tweet_mode=extended"), It.IsAny<AuthenticationHeaderValue>()))
                .ReturnsAsync(tweets);
            downloader.Setup(x => x.GetWebContent<List<Tweet>, ErrorsTwitter>(It.Is<string>(s => s != "https://api.twitter.com/1.1/statuses/user_timeline.json?include_entities=true&count=200&include_rts=true&tweet_mode=extended"), It.IsAny<AuthenticationHeaderValue>()))
                .ReturnsAsync(JsonConvert.DeserializeObject<List<Tweet>>("[]"));
            ConnectorTask connectorTask = new ConnectorTask
            {
                TenantId = "tenantId",
                JobId = "j1",
                TaskId = "t1",
                StartTime = new DateTime(2018, 12, 01),
                EndTime = new DateTime(2019, 05, 01),
                DirtyEntities = null,
                BlobSasUri = "dummyUri"
            };

            SourceInfoTwitter sourceInfo = new SourceInfoTwitter()
            {
                SinceId = "0",
            };
            jobProcessor = new JobProcessorTwitter(downloader.Object, uploader.Object, new TwitterSchemaToItemMapper());
            var listTweets = await jobProcessor.FetchData(connectorTask, JsonConvert.SerializeObject(sourceInfo));

            tweets.RemoveAll(t => DateTime.Compare(DateTime.ParseExact(t.CreatedAt, "ddd MMM dd HH:mm:ss +ffff yyyy", new System.Globalization.CultureInfo("en-US")), connectorTask.StartTime) < 0);
            tweets.RemoveAll(t => DateTime.Compare(DateTime.ParseExact(t.CreatedAt, "ddd MMM dd HH:mm:ss +ffff yyyy", new System.Globalization.CultureInfo("en-US")), connectorTask.EndTime) > 0);
            Assert.AreEqual(listTweets.Count, tweets.Count);
            mockRepo.VerifyAll();
        }

        [TestMethod]
        public async Task FetchData_WhenSinceIdisMax_ThenNoDataFetched()
        {
            var tweets = JsonConvert.DeserializeObject<List<Tweet>>(File.ReadAllText(@"FakeTweets.json"));
            var max = tweets.Select(t => long.Parse(t.Tweetid)).ToList<long>().Max().ToString();
            downloader.Setup(x => x.GetWebContent<List<Tweet>, ErrorsTwitter>(It.Is<string>(s => s == $"https://api.twitter.com/1.1/statuses/user_timeline.json?include_entities=true&count=200&include_rts=true&sinceId={max}"), It.IsAny<AuthenticationHeaderValue>()))
                .ReturnsAsync(JsonConvert.DeserializeObject<List<Tweet>>("[]"));
            ConnectorTask connectorTask = new ConnectorTask
            {
                TenantId = "tenantId",
                JobId = "j1",
                TaskId = "t1",
                StartTime = DateTime.UtcNow.AddMonths(-2),
                EndTime = DateTime.UtcNow,
                DirtyEntities = null,
                BlobSasUri = "dummyUri"
            };

            SourceInfoTwitter sourceInfo = new SourceInfoTwitter()
            {
                SinceId = max,
            };
            jobProcessor = new JobProcessorTwitter(downloader.Object, uploader.Object, new TwitterSchemaToItemMapper());
            var listTweets = await jobProcessor.FetchData(connectorTask, JsonConvert.SerializeObject(sourceInfo));
            Assert.IsTrue(listTweets.Count == 0);
            downloader.Verify(m => m.GetWebContent<List<Tweet>, ErrorsTwitter>(It.IsAny<string>(), It.IsAny<AuthenticationHeaderValue>()), Times.Once);
            uploader.Verify(x => x.UploadItem(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Item>()), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task FetchData_WhenErrorReturned_ThenExceptionThrown()
        {
            var tweets = JsonConvert.DeserializeObject<List<Tweet>>(File.ReadAllText(@"FakeTweets.json"));
            var max = tweets.Select(t => long.Parse(t.Tweetid)).ToList<long>().Max().ToString();
            var error = new ErrorsTwitter()
            {
                Errors = new List<ErrorTypeTwitter>
                {
                    new ErrorTypeTwitter()
                    {
                        Code = 400,
                        ErrorMessage = "Bad Request"                        
                    }
                },
            };

            downloader.Setup(x => x.GetWebContent<List<Tweet>, ErrorsTwitter>(It.IsAny<string>(), It.IsAny<AuthenticationHeaderValue>()))
                .Throws(new HttpRequestException());
            ConnectorTask connectorTask = new ConnectorTask
            {
                TenantId = "tenantId",
                JobId = "j1",
                TaskId = "t1",
                StartTime = DateTime.UtcNow.AddMonths(-2),
                EndTime = DateTime.UtcNow,
                DirtyEntities = null,
                BlobSasUri = "dummyUri"
            };

            SourceInfoTwitter sourceInfo = new SourceInfoTwitter()
            {
                SinceId = max,
            };
            jobProcessor = new JobProcessorTwitter(downloader.Object, uploader.Object, new TwitterSchemaToItemMapper());
            var list = await jobProcessor.FetchData(connectorTask, JsonConvert.SerializeObject(sourceInfo));
        }
    }
}
