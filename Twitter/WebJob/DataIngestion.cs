// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.TwitterConnector
{
    using Newtonsoft.Json;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System;
    using Microsoft.WindowsAzure.Storage.Table;
    using System.Linq.Expressions;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Sample.Connector;
    using Sample.TwitterSDK;
    using System.Linq;

    public class DataIngestion
    {
        private readonly HttpClient httpClient;
        private CloudTable PageJobMappingTable;
        private readonly AzureTableProvider azureTableProvider;

        public DataIngestion(HttpClient httpClient, AzureTableProvider azureTableProvider, CloudTable cloudTable)
        {
            this.httpClient = httpClient;
            this.PageJobMappingTable = cloudTable;
            this.azureTableProvider = azureTableProvider;
        }

        public async Task Execute(string jobMessage)
        {
            ConnectorTask taskInfo = JsonConvert.DeserializeObject<ConnectorTask>(jobMessage);
            IEventApiClient eventApiClient = new EventApiClient(new Auth(Settings.AAdAppId, Settings.AAdAppSecret), Settings.EventAPIBaseUrl);
            IUploader uploader = new BlobUploader(taskInfo.BlobSasUri);
            string sourceInfo = await GetSourceInfoFromTable(taskInfo);
            Trace.TraceInformation($"Fetched job info from PageJobEntity Table for JobId: {taskInfo.JobId} and TaskId: {taskInfo.TaskId}");
            Status status;
            List<ItemMetadata> itemMetadata = new List<ItemMetadata>();
            IDownloader downloader = new Downloader();
            TwitterSchemaToItemMapper itemMapper = new TwitterSchemaToItemMapper();
            JobProcessorTwitter jobProcessor = new JobProcessorTwitter(downloader, uploader, itemMapper);
            try
            {
                itemMetadata = await jobProcessor.FetchData(taskInfo, sourceInfo);

                SourceInfoTwitter twitterSourceInfo = JsonConvert.DeserializeObject<SourceInfoTwitter>(sourceInfo);
                var listId = itemMetadata.Select(t => long.Parse(t.id)).ToList();
                twitterSourceInfo.SinceId = listId.Count == 0 ? twitterSourceInfo.SinceId : listId.Max().ToString();
                await jobProcessor.UpdateSourceInfo(taskInfo, twitterSourceInfo);
                status = Status.Success;
                Trace.TraceInformation($"Successfully completed Job Execution, JobId:{taskInfo.JobId}, TaskId:{taskInfo.TaskId}");
            }
            catch (HttpRequestException e)
            {
                status = Status.TemporaryFailure;
                Trace.TraceError($"Connectivity Error, JobId:{taskInfo.JobId}, TaskId:{taskInfo.TaskId}, Error: {e.Message}, ErrorStackTrace: {e.StackTrace}");
            }
            catch (Exception e)
            {
                status = Status.PermanentFailure;
                Trace.TraceError($"Unknown Failure, Requires Attention, JobId:{taskInfo.JobId}, TaskId:{taskInfo.TaskId}, Error: {e.Message}, ErrorStackTrace: {e.StackTrace}");
            }
            itemMetadata.OrderBy(i => i.id);
            await eventApiClient.OnDownloadCompleteAsync(taskInfo.TenantId, taskInfo.JobId, taskInfo.TaskId, status, itemMetadata);
        }

        private async Task<string> GetSourceInfoFromTable(ConnectorTask taskInfo)
        {
            Expression<Func<PageJobEntity, bool>> filter = (entity => entity.RowKey == taskInfo.JobId);
            List<PageJobEntity> pageJobEntityList = await azureTableProvider.QueryEntitiesAsync<PageJobEntity>(PageJobMappingTable, filter);
            PageJobEntity pageJobEntity = pageJobEntityList?[0];
            return pageJobEntity.SourceInfo;
        }
    }
}
