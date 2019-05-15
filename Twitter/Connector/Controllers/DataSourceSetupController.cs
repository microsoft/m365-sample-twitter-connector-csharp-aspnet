// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.TwitterConnector
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Linq.Expressions;
    using Microsoft.WindowsAzure.Storage.Table;
    using Connector;
    using Sample.TwitterSDK;

    /// <summary>
    /// API controller for all native connector setups
    /// </summary>
    [ApiAuthorizationModule]
    public class DataSourceSetupController : ApiController
    {
        private const string TokenParam = "temporaryAccessCode";

        private AzureTableProvider azureTableProviderInstance;

        private CloudTable PageJobMappingTable;

        private readonly IConnectorSourceProvider sourceProvider;

        public DataSourceSetupController()
        {
            azureTableProviderInstance = new AzureTableProvider(Settings.StorageAccountConnectionString);
            var azureTableProvider = new AzureTableProvider(Settings.StorageAccountConnectionString);
            var client = new RestApiRepository(SettingsTwitter.TwitterAuthEndPoint);
            sourceProvider = new TwitterProvider(azureTableProvider, client, new TwitterAuthProvider(client, azureTableProvider));
        }           
        
        [HttpGet]
        [Route("api/ConnectorSetup/OAuthUrl")]
        public async Task<string> GetOAuthUrl([FromUri] string jobType, [FromUri] string redirectUrl)
        {
            string authUrl = await sourceProvider.GetOAuthUrl(redirectUrl);

            Trace.TraceInformation("GetOAuthUrl url generated successfully for Jobtype {0}", jobType);
            return authUrl;
        }

        [HttpPost]
        [Route("api/ConnectorSetup/StoreToken")]
        public async Task<bool> StoreToken([FromBody] Dictionary<string, string> tokenInfo)
        {
            await sourceProvider.StoreOAuthToken(tokenInfo[TokenParam], tokenInfo["redirectUrl"], tokenInfo["jobId"]);
            return true;
        }

        /// <summary>
        /// Returns the list of entities for native connector job type
        /// </summary>
        /// <param name="jobType">Native Connector job type</param>
        /// <param name="jobId">job Id for current job</param>
        /// <returns>List of jobs owned by the tenant.</returns>
        [HttpGet]
        [Route("api/ConnectorSetup/GetEntities")]
        public async Task<IEnumerable<ConnectorEntity>> Get([FromUri] string jobType, [FromUri] string jobId)
        {
            CloudTable jobMappingTable = azureTableProviderInstance.GetAzureTableReference(Settings.PageJobMappingTableName);

            Trace.TraceInformation("Getting connector Entities for JobType {0}", jobType.ToString());

            IEnumerable<ConnectorEntity> entities = await sourceProvider.GetEntities(jobId);

            Trace.TraceInformation("Entities retrieved: {0}", entities?.Count());

            if (entities != null)
            {
                foreach (ConnectorEntity entity in entities)
                {
                    Expression<Func<PageJobEntity, bool>> filter = (e => e.PartitionKey == entity.Id);
                    List<PageJobEntity> pageJobEntityList = await azureTableProviderInstance.QueryEntitiesAsync<PageJobEntity>(jobMappingTable, filter);

                    entity.AlreadyUsed = pageJobEntityList.Any();
                }
            }

            IEnumerable<ConnectorEntity> response = new List<ConnectorEntity>();
            response = entities;
            return response;
        }

        /// <summary>
        /// Store the selected page
        /// </summary>
        /// <param name="page">page data</param>
        /// <param name="jobId">jobId partition key</param>
        /// <param name="jobType">type of job</param>
        /// <param name="tenantId">tenant id</param>
        /// <returns>if source is saved successfully</returns>
        [HttpPost]
        [Route("api/ConnectorSetup/SavePage")]
        public async Task<bool> SavePage([FromBody] ConnectorEntity page, [FromUri] string jobId, [FromUri] string tenantId)
        {
            Expression < Func < PageJobEntity, bool>> filter = (entity => entity.RowKey == $"{jobId}");
            PageJobMappingTable = azureTableProviderInstance.GetAzureTableReference(Settings.PageJobMappingTableName);
            List<PageJobEntity> pageJobEntityList = await azureTableProviderInstance.QueryEntitiesAsync<PageJobEntity>(PageJobMappingTable, filter);
            PageJobEntity pageJobEntity = pageJobEntityList?[0];
            Trace.TraceInformation("Job Setup complete page succesfully saved for jobId: {0}", jobId);

            try
            {
                Trace.TraceInformation("Job with JobId: {0} subscribing to webhook", jobId);
                bool subscribed = await sourceProvider.Subscribe(pageJobEntity.SourceInfo);
                Trace.TraceInformation("Job with JobId: {0} successfully subscribed to webhook", jobId);
            }
            catch (Exception e)
            {
                Trace.TraceInformation("Job with JobId: {0} subscribed to webhook failed with error: {1}", jobId, e.Message);
                return false;
            }

            return true;
        }

        [HttpGet]
        [Route("api/ConnectorSetup/DeleteToken")]
        public bool DeleteToken([FromUri] string jobType, [FromUri] string jobId)
        {
            sourceProvider.DeleteToken(jobType, jobId);
            Trace.TraceInformation("Token deleted succesfully. JobType: {0}", jobType);
            return true;
        }
    }
}