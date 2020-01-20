// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.TwitterConnector
{
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.WindowsAzure.Storage.Table;
    using Sample.Connector;
    using Sample.TwitterSDK;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Http;

    [ApiAuthorizationModule]
    public class ConfigurationController : ApiController
    {
        private readonly AzureTableProvider azureTableProvider;
        private CloudTable SettingsTable;

        public ConfigurationController()
        {
            this.azureTableProvider = new AzureTableProvider(Settings.StorageAccountConnectionString);
        }

        /// <summary>
        /// Configure settings
        /// </summary>
        /// <param name="configSettings">Configuration settings</param>
        /// <returns>if configuration is saved successfully</returns>
        [HttpPost]
        [Route("api/Configuration")]
        public async Task<bool> Configure([FromBody] Dictionary<string, string> configSettings)
        {
            SettingsTable = azureTableProvider.GetAzureTableReference(Settings.ConfigurationSettingsTableName);

            if (!string.IsNullOrEmpty(configSettings["TwitterApiKeyValue"]))
            {
                SettingsTwitter.TwitterApiKey = configSettings["TwitterApiKeyValue"];
                await azureTableProvider.InsertOrReplaceEntityAsync(SettingsTable, new ConfigurationSettingsEntity("TwitterApiKey", configSettings["TwitterApiKeyValue"]));
            }

            if (!string.IsNullOrEmpty(configSettings["TwitterApiSecretKeyValue"]))
            {
                SettingsTwitter.TwitterApiSecretKey = configSettings["TwitterApiSecretKeyValue"];
                await azureTableProvider.InsertOrReplaceEntityAsync(SettingsTable, new ConfigurationSettingsEntity("TwitterApiSecretKey", configSettings["TwitterApiSecretKeyValue"]));
            }

            if (!string.IsNullOrEmpty(configSettings["TwitterAccessTokenValue"]))
            {
                SettingsTwitter.TwitterAccessToken = configSettings["TwitterAccessTokenValue"];
                await azureTableProvider.InsertOrReplaceEntityAsync(SettingsTable, new ConfigurationSettingsEntity("TwitterAccessToken", configSettings["TwitterAccessTokenValue"]));
            }

            if (!string.IsNullOrEmpty(configSettings["TwitterAccessTokenSecretValue"]))
            {
                SettingsTwitter.TwitterAccessTokenSecret = configSettings["TwitterAccessTokenSecretValue"];
                await azureTableProvider.InsertOrReplaceEntityAsync(SettingsTable, new ConfigurationSettingsEntity("TwitterAccessTokenSecret", configSettings["TwitterAccessTokenSecretValue"]));
            }

            if (!string.IsNullOrEmpty(configSettings["AADAppIdValue"]))
            {
                Settings.AAdAppId = configSettings["AADAppIdValue"];
                await azureTableProvider.InsertOrReplaceEntityAsync(SettingsTable, new ConfigurationSettingsEntity("AAdAppId", configSettings["AADAppIdValue"]));
            }

            if (!string.IsNullOrEmpty(configSettings["AADAppSecretValue"]))
            {
                Settings.AAdAppSecret = configSettings["AADAppSecretValue"];
                await azureTableProvider.InsertOrReplaceEntityAsync(SettingsTable, new ConfigurationSettingsEntity("AAdAppSecret", configSettings["AADAppSecretValue"]));
            }
            return true;
        }       

        /// <summary>
        /// Get configuration settings
        /// </summary>
        /// <returns>configuration settings</returns>
        [HttpGet]
        [Route("api/Configuration")]
        public Task<Dictionary<string, string>> GetConfiguration()
        {
            Dictionary<string, string> configurationSettings = new Dictionary<string, string>();
            configurationSettings.Add("TwitterApiKeyValue", SettingsTwitter.TwitterApiKey);
            configurationSettings.Add("TwitterApiSecretKeyValue", SettingsTwitter.TwitterApiSecretKey);
            configurationSettings.Add("TwitterAccessTokenValue", SettingsTwitter.TwitterAccessToken);
            configurationSettings.Add("TwitterAccessTokenSecretValue", SettingsTwitter.TwitterAccessTokenSecret);
            configurationSettings.Add("AADAppSecretValue", Settings.AAdAppSecret);
            configurationSettings.Add("AADAppIdValue", Settings.AAdAppId);
            return Task.FromResult(configurationSettings);
        }
    }
}