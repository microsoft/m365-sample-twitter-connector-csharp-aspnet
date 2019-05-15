using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sample.TwitterSDK;
using Sample.Connector;
using Moq;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace ConnectorTest
{
    /// <summary>
    /// Summary description for TwitterProviderTest
    /// </summary>
    [TestClass]
    public class TwitterProviderTest
    {
        private TwitterProvider twitterProvider;
        private AzureTableProvider azureTableProvider;
        private Mock<RestApiRepository> mockIRestApiRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            azureTableProvider = new AzureTableProvider(Settings.StorageAccountConnectionString);
            mockIRestApiRepository = new Mock<RestApiRepository>(SettingsTwitter.TwitterAuthEndPoint);
            var twitterAuthProvider = new Mock<TwitterAuthProvider>(mockIRestApiRepository.Object, azureTableProvider);
            twitterProvider = new TwitterProvider(azureTableProvider, mockIRestApiRepository.Object, twitterAuthProvider.Object);
        }

        [TestMethod]
        public void GetAuthTokenForResourceTest()
        {
            Assert.IsNotNull(twitterProvider.GetAuthTokenForResource(It.IsAny<string>(), It.IsAny<string>()).Result);
        }

        [TestMethod]
        public void SubscribeTest()
        {
            Assert.IsTrue(twitterProvider.Subscribe(It.IsAny<string>()).Result);
        }

        [TestMethod]
        public void UnsubscribeTest()
        {
            Assert.IsTrue(twitterProvider.Unsubscribe(It.IsAny<string>()).Result);
        }
    }
}
