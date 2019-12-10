// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.TwitterSDK
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;
    using Sample.Connector;

    public class TwitterProvider : ConnectorSourceProvider
    {
        private TwitterAuthProvider twitterAuthProvider;

        private IRestApiRepository Client;

        public TwitterProvider(AzureTableProvider azureTableProviderInstance, IRestApiRepository Client, TwitterAuthProvider twitterAuthProvider)
            : base(azureTableProviderInstance, azureTableProviderInstance.GetAzureTableReference(Settings.TokenTableName))
        {
            this.Client = Client;
            this.twitterAuthProvider = twitterAuthProvider;
        }

        public override async Task<string> GetOAuthUrl(string redirectUrl)
        {
            return string.Format($"{SettingsTwitter.TwitterAuthEndPoint}/oauth/authenticate?oauth_callback={redirectUrl}&oauth_token={await GetOAuthToken()}");
        }

        public override async Task StoreOAuthToken(string accessCode, string redirectUrl, string jobId)
        {
            var requestTokens = JsonConvert.DeserializeObject<Dictionary<string, string>>(await GetTokenFromStorage(Settings.TenantId, "Twitter"));
            var accessToken = await this.twitterAuthProvider.GetAccessToken(accessCode, redirectUrl, requestTokens);
            await this.SavePageJobEntity(jobId, accessToken);
        }        

        public override async Task<IEnumerable<ConnectorEntity>> GetEntities(string jobId)
        {
            List<ConnectorEntity> entities = new List<ConnectorEntity>();

            var PageJobMappingTable = azureTableProvider.GetAzureTableReference(Settings.PageJobMappingTableName);
            Expression<Func<PageJobEntity, bool>> filter = (entity => entity.RowKey == $"{jobId}");
            List<PageJobEntity> pageJobEntityList = await azureTableProvider.QueryEntitiesAsync<PageJobEntity>(PageJobMappingTable, filter);
            PageJobEntity pageJobEntity = pageJobEntityList?[0];

            string userToken = twitterAuthProvider.GetUserTokenForjobId(pageJobEntity);
            AccountTwitter AccountTwitter = await twitterAuthProvider.GetAuthorizedAccount(userToken);

            entities.Add(new ConnectorEntity {
                Id = AccountTwitter.Id,
                Name = AccountTwitter.Name
            });
            
            return entities;
        }
        
        /// <summary>
        /// Subscribe page feed through webhooks
        /// </summary>
        public override async Task<bool> Subscribe(string sourceInfoJson)
        {
            return await Task.FromResult(true);
        }

        public override async Task<string> GetAuthTokenForResource(string resourceId, string jobId)
        {
            return await Task.FromResult(string.Empty);
        }
        
        public override async Task<bool> Unsubscribe(string sourceInfo)
        {
            return await Task.FromResult(true);
        }

        private async Task<string> GetOAuthToken()
        {
            var requestToken = string.Empty;
            string ClientTokens = await this.twitterAuthProvider.GetOAuthToken();
            var result = new Dictionary<string, string>();
            foreach (var pair in ClientTokens.Split('&'))
            {
                if (pair.Split('=')[0].Equals("oauth_token"))
                {
                    requestToken = pair.Split('=')[1];
                    result.Add("ClientToken", requestToken);
                }
                if (pair.Split('=')[0].Equals("oauth_token_secret"))
                {
                    result.Add("ClientSecret", pair.Split('=')[1]);
                }
            }

            await AddTokenIntoStorage(Settings.TenantId, JsonConvert.SerializeObject(result), "Twitter");
            return requestToken;
        }

        private async Task SavePageJobEntity(string jobId, string accessToken)
        {
            string page_id = string.Empty;
            SourceInfoTwitter sourceInfo = new SourceInfoTwitter();

            var pageJobMappingTable = azureTableProvider.GetAzureTableReference(Settings.PageJobMappingTableName);
            Expression<Func<PageJobEntity, bool>> filter = (entity => entity.RowKey == $"{jobId}");
            List<PageJobEntity> pageJobEntityList = await azureTableProvider.QueryEntitiesAsync<PageJobEntity>(pageJobMappingTable, filter);
            PageJobEntity pageJobEntity = pageJobEntityList?[0];
            SourceInfoTwitter SourceInfoTwitter = JsonConvert.DeserializeObject<SourceInfoTwitter>(pageJobEntity?.SourceInfo);
            if (SourceInfoTwitter != null)
            {
                sourceInfo.SinceId = SourceInfoTwitter.SinceId;
            }
            else
            {
                sourceInfo.SinceId = "0";
            }

            foreach (var pair in accessToken.Split('&'))
            {
                var keys = pair.Split('=');
                if (keys[0].Equals("oauth_token"))
                {
                    sourceInfo.ClientToken = keys[1];
                }
                if (keys[0].Equals("oauth_token_secret"))
                {
                    sourceInfo.ClientSecret = keys[1];
                }
                if (keys[0].Equals("user_id"))
                {
                    page_id = keys[1];
                }
            }

            var PageJobMappingTable = azureTableProvider.GetAzureTableReference(Settings.PageJobMappingTableName);
            var pageJobEntity = new PageJobEntity(page_id, jobId)
            {
                SourceInfo = JsonConvert.SerializeObject(sourceInfo)
            };

            await azureTableProvider.InsertOrReplaceEntityAsync(PageJobMappingTable, pageJobEntity);
        }

    }
  }
