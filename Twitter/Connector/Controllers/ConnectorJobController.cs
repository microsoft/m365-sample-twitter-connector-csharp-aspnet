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
    using System.Net;
    using System.Net.Http;
    using Sample.TwitterSDK;
    using Newtonsoft.Json;


    [ApiAuthorizationModule]
    public class ConnectorJobController : ApiController
    {
        private AzureTableProvider azureTableProvider;
        private IConnectorSourceProvider connectorSourceProvider;

        public ConnectorJobController()
        {
            azureTableProvider = new AzureTableProvider(Settings.StorageAccountConnectionString);
            var client = new RestApiRepository(SettingsTwitter.TwitterAuthEndPoint);
            connectorSourceProvider = new TwitterProvider(azureTableProvider, client, new TwitterAuthProvider(client, azureTableProvider));
        }


        /// <summary>
        /// Final job setup for Connector service
        /// </summary>
        /// <param name="jobId">job Id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("job/IsCreated")]
        public async Task<HttpResponseMessage> ConnectorOAuth([FromUri] string jobId)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            var configuration = new HttpConfiguration();
            request.SetConfiguration(configuration);
            CloudTable jobMappingTable = azureTableProvider.GetAzureTableReference(Settings.PageJobMappingTableName);

            Expression<Func<PageJobEntity, bool>> filter = (entity => entity.RowKey == jobId);
            List<PageJobEntity> pageJobEntityList = await azureTableProvider.QueryEntitiesAsync<PageJobEntity>(jobMappingTable, filter);

            if (!pageJobEntityList.Any())
            {
                return request.CreateResponse<JobCreationResponse>(HttpStatusCode.OK, new JobCreationResponse(false, null));

            }

            Trace.TraceInformation("Job with JobId: {0} successfully set up", jobId);
            PageJobEntity pageJobEntity = pageJobEntityList?[0];
            return request.CreateResponse<JobCreationResponse>(HttpStatusCode.OK, new JobCreationResponse(true, null));
        }

        /// <summary>
        /// Delete job page
        /// </summary>
        /// <param name="jobId">job Id</param>
        /// <returns>success or failure</returns>
        [HttpDelete]
        [Route("job/OnDeleted")]
        public async Task<HttpResponseMessage> DeleteJob([FromUri] string jobId)
        {
            CloudTable jobMappingTable = azureTableProvider.GetAzureTableReference(Settings.PageJobMappingTableName);

            Expression<Func<PageJobEntity, bool>> filter = (entity => entity.RowKey == jobId);
            List<PageJobEntity> pageJobEntityList = await azureTableProvider.QueryEntitiesAsync<PageJobEntity>(jobMappingTable, filter);

            if (!pageJobEntityList.Any())
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            PageJobEntity pageJobEntity = pageJobEntityList?[0];

            bool unsubscribed = await connectorSourceProvider.Unsubscribe(pageJobEntity.SourceInfo);
            Trace.TraceInformation("Job with JobId: {0} successfully unsubscribed to webhook", jobId);

            if (unsubscribed == false)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }

            await azureTableProvider.DeleteEntityAsync<PageJobEntity>(jobMappingTable, pageJobEntity);
            Trace.TraceInformation("Job with JobId: {0} successfully deleted", jobId);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
        /// <summary>
        /// Remediate job 
        /// </summary>
        /// <param name="jobId">job Id</param>
        /// <param name="RemediationType">Remediationtype</param>
        /// <returns>success or failure</returns>
        [HttpGet]
        [Route("job/OnRemediation")]
        public Task<HttpResponseMessage> RemediateJob([FromUri] string jobId, JobRemediationType remediationType)
        {
            return Task.FromResult<HttpResponseMessage>(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}