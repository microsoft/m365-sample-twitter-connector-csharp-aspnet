// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.TwitterSDK
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;
    using Sample.Connector;
    using Sample.TwitterSDK.Auth;

    public class TwitterAuthProvider
    {
        private IRestApiRepository Client;
        protected AzureTableProvider azureTableProvider;
        private CloudTable PageJobMappingTable;
        public TwitterAuthProvider(IRestApiRepository httpClient, AzureTableProvider azureTableProvider)
        {
            this.Client = httpClient;
            this.azureTableProvider = azureTableProvider;
            PageJobMappingTable = azureTableProvider.GetAzureTableReference(Settings.PageJobMappingTableName);
        }

        public async Task<string> GetOAuthToken()
        {
            string url = string.Format($"{SettingsTwitter.TwitterEndPoint}/oauth/request_token");
            var headers = new Dictionary<string, string>();
            string authToken = GetToken(url, HttpMethod.Post.ToString().ToUpperInvariant(), SettingsTwitter.TwitterClientToken, SettingsTwitter.TwitterClientSecret, null);
            headers.Add("Authorization", "OAuth " + authToken);
            string ClientTokens = await this.Client.PostRequestAsync<string, string>("oauth/request_token", headers, string.Empty, CancellationToken.None);
            return ClientTokens;
        }

        public string GetUserTokenForjobId(PageJobEntity pageJobEntity)
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add("format", "json");
            queryParams.Add("include_email", "true");
            queryParams.Add("include_entities", "false");

            SourceInfoTwitter SourceInfoTwitter = JsonConvert.DeserializeObject<SourceInfoTwitter>(pageJobEntity.SourceInfo);
            string clientToken = SourceInfoTwitter.ClientToken;
            string clientSecret = SourceInfoTwitter.ClientSecret;
            string userToken = GetToken(SettingsTwitter.TwitterEndPoint + "/1.1/account/verify_credentials.json", HttpMethod.Get.ToString().ToUpperInvariant(), clientToken, clientSecret, queryParams);
            return userToken;
        }

        public async Task<string> GetAccessToken(string accessCode, string redirectUrl, Dictionary<string, string> requestTokens)
        {
            string url = string.Format($"{SettingsTwitter.TwitterEndPoint}/oauth/access_token");
            var queryParams = new Dictionary<string, string>();
            queryParams.Add("oauth_verifier", accessCode);
            queryParams.Add("oauth_callback", redirectUrl);

            OAuth1Token oAuth1Token = new OAuth1Token(SettingsTwitter.TwitterClientToken, SettingsTwitter.TwitterClientSecret, requestTokens.Where(k => k.Key == "ClientToken").FirstOrDefault().Value, requestTokens.Where(k => k.Key == "ClientSecret").FirstOrDefault().Value);
            OAuth1Helper oAuth1Helper = new OAuth1Helper(url, oAuth1Token, HttpMethod.Post.ToString().ToUpperInvariant());
            var qstr = oAuth1Helper.GetQueryString(queryParams);
            string tempToken = oAuth1Helper.GenerateAuthorizationHeader();
            var requestHeaders = new Dictionary<string, string>();
            requestHeaders.Add("Authorization", "OAuth " + tempToken);

            return await this.Client.PostRequestAsync<Dictionary<string, string>, string>(url, requestHeaders, queryParams, CancellationToken.None);
        }

        public async Task<AccountTwitter> GetAuthorizedAccount(string userToken)
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add("format", "json");
            queryParams.Add("include_email", "true");
            queryParams.Add("include_entities", "false");

            var headers = new Dictionary<string, string>();
            headers.Add("Authorization", "OAuth " + userToken);

            AccountTwitter response = await this.Client.GetRequestAsync<AccountTwitter>("1.1/account/verify_credentials.json", headers, queryParams, CancellationToken.None);
            return response;
        }

        private static OAuth1Helper getOAuthHelper(string url, string httpMethod, string token, string secret)
        {
            OAuth1Token oAuth1Token = new OAuth1Token(SettingsTwitter.TwitterAppId, SettingsTwitter.TwitterAppSecret, token, secret);
            OAuth1Helper oAuth1Helper = new OAuth1Helper(url, oAuth1Token, httpMethod);
            return oAuth1Helper;
        }

        private string GetToken(string url, string httpMethod, string token, string secret, Dictionary<string, string> query)
        {
            OAuth1Helper oAuth1Helper = getOAuthHelper(url, httpMethod, token, secret);
            var qstr = oAuth1Helper.GetQueryString(query);
            return oAuth1Helper.GenerateAuthorizationHeader();
        }
    }
}
