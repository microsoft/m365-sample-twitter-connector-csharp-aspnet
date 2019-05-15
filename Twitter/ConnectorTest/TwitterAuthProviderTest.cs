using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sample.Connector;
using System.Threading.Tasks;
using Moq;
using Sample.TwitterSDK;
using System.Threading;
using Newtonsoft.Json;

namespace ConnectorTest
{
    /// <summary>
    /// Summary description for TwitterAuthProviderTest
    /// </summary>
    [TestClass]
    public class TwitterAuthProviderTest
    {
        private AzureTableProvider azureTableProvider;
        private Mock<IRestApiRepository> restApiRepositoryMock;

        [TestInitialize]
        public void TestInitialize()
        {
            azureTableProvider = new AzureTableProvider(Settings.StorageAccountConnectionString);
            this.restApiRepositoryMock = new Mock<IRestApiRepository>();
            var twitterAuthProvider = new Mock<TwitterAuthProvider>(this.restApiRepositoryMock.Object, azureTableProvider);
        }

        [TestMethod]
        public async Task GetAccessTokenTest()
        {
            Settings.TenantId = "tenant123";
            restApiRepositoryMock.Setup(x => x.PostRequestAsync<Dictionary<string, string>, string>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<Dictionary<string, string>>(), CancellationToken.None))
                .ReturnsAsync("access_token");
            var twitterAuthprovider = new TwitterAuthProvider(restApiRepositoryMock.Object, azureTableProvider);
            string access_token = await twitterAuthprovider.GetAccessToken("ACCESS_CODE", "REDIRECT_URL", new Dictionary<string, string>());
            Assert.AreEqual("access_token", access_token);
            restApiRepositoryMock.Verify(x => x.PostRequestAsync<Dictionary<string, string>, string>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<Dictionary<string, string>>(), CancellationToken.None), Times.Once);
        }



        [TestMethod]
        public async Task GetOAuthUrlTest()
        {
            Settings.TenantId = "tenant123";
            var tokenResponse = "oauth_token=TOKEN&oauth_token_secret=SECRET&oauth_callback_confirmed=true";
            restApiRepositoryMock.Setup(x => x.PostRequestAsync<string, string>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), CancellationToken.None))
                .ReturnsAsync(tokenResponse);
            var twitterAuthProvider = new TwitterAuthProvider(restApiRepositoryMock.Object, azureTableProvider);
            var tokenString = await twitterAuthProvider.GetOAuthToken();
            Assert.AreEqual(tokenResponse, tokenString);
            restApiRepositoryMock.Verify(x => x.PostRequestAsync<string, string>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), CancellationToken.None), Times.Once);
        }

        [TestMethod]
        public async Task getAuthorizedAccountTest()
        {
            Settings.TenantId = "tenant123";
            var twitterAccount = new AccountTwitter()
            {
                Id = "123",
                Name = "test_acc",
                EmaildId = "test@mail.com"
            };
            restApiRepositoryMock.Setup(x => x.GetRequestAsync<AccountTwitter>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<Dictionary<string, string>>(), CancellationToken.None))
                .ReturnsAsync(twitterAccount);
            var twitterAuthProvider = new TwitterAuthProvider(restApiRepositoryMock.Object, azureTableProvider);
            var entities = await twitterAuthProvider.GetAuthorizedAccount(It.IsAny<string>());
            Assert.AreEqual(twitterAccount, entities);
            restApiRepositoryMock.Verify(x => x.GetRequestAsync<AccountTwitter>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<Dictionary<string, string>>(), CancellationToken.None), Times.Once);
        }

        [TestMethod]
        public void GetUserTokenForjobIdTest()
        {
            SourceInfoTwitter sourceinfo = new SourceInfoTwitter();
            sourceinfo.ClientSecret = "CLIENT_SECRET";
            sourceinfo.ClientToken = "CLIENT_TOKEN";
            PageJobEntity pageJobEntity = new PageJobEntity()
            {
                PartitionKey = "123",
                RowKey = "abc",
                SourceInfo = JsonConvert.SerializeObject(sourceinfo),
            };
            var twitterAuthProvider = new TwitterAuthProvider(restApiRepositoryMock.Object, azureTableProvider);
            var token = twitterAuthProvider.GetUserTokenForjobId(pageJobEntity);
            Assert.IsTrue(token.Contains("CLIENT_TOKEN"));
        }
}
}
