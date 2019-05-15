// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.TwitterConnector
{
    using Connector;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;

    [ApiAuthorizationModule]
    public class DataIngestionController : ApiController
    {
        private AzureTableProvider azureTableProviderInstance;
        private AzureStorageQueueProvider queueProvider;
        private CloudTable pageJobMappingTable;
        
        public DataIngestionController()
        {
            this.queueProvider = new AzureStorageQueueProvider(Settings.StorageAccountConnectionString, Settings.QueueName);
            this.azureTableProviderInstance = new AzureTableProvider(Settings.StorageAccountConnectionString);
        }

        /// <summary>
        /// schedules the task for download and transform.
        /// </summary>
        /// <param name="request">Callback request body from M365 Connector platform</param>
        /// <returns></returns>
        [HttpPost]
        [Route("preview/OnDataRequest")]
        [Route("api/DataIngestion/ScheduleTask")]
        public async Task<HttpResponseMessage> ScheduleTask([FromBody] ScheduleTaskRequest request)
        {
            Trace.TraceInformation($"Request came to Web for JobId: {request.JobId} and TaskId: {request.TaskId}");
            PageJobEntity entity = await GetJobIdFromTable(request.JobId);
            if (entity == null)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            else
            {
                await queueProvider.InsertMessageAsync(JsonConvert.SerializeObject(new ConnectorTask
                {
                    TenantId = Settings.TenantId,
                    JobId = request.JobId,
                    TaskId = request.TaskId,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    DirtyEntities = request.DirtyEntities,
                    BlobSasUri = request.BlobSasUri
                }));
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        }

        private async Task<PageJobEntity> GetJobIdFromTable(string jobId)
        {
            Expression<Func<PageJobEntity, bool>> filter = (entity => entity.RowKey == jobId);
            pageJobMappingTable = azureTableProviderInstance.GetAzureTableReference(Settings.PageJobMappingTableName);
            List<PageJobEntity> pageJobEntityList = await azureTableProviderInstance.QueryEntitiesAsync<PageJobEntity>(pageJobMappingTable, filter);
            return pageJobEntityList?[0];
        }
    }
}