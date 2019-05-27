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

            if (!string.IsNullOrEmpty(configSettings["AADAppUriValue"]))
            {
                Settings.AADAppUri = configSettings["AADAppUriValue"];
                await azureTableProvider.InsertOrReplaceEntityAsync(SettingsTable, new ConfigurationSettingsEntity("AADAppUri", configSettings["AADAppUriValue"]));
            }

            if (!string.IsNullOrEmpty(configSettings["InstrumentationKeyValue"]))
            {
                Settings.APPINSIGHTS_INSTRUMENTATIONKEY = configSettings["InstrumentationKeyValue"];
                await azureTableProvider.InsertOrReplaceEntityAsync(SettingsTable, new ConfigurationSettingsEntity("APPINSIGHTS_INSTRUMENTATIONKEY", configSettings["InstrumentationKeyValue"]));
                TelemetryConfiguration.Active.InstrumentationKey = configSettings["InstrumentationKeyValue"];
            }
            return true;
        }
    }
}