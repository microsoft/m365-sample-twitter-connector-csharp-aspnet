// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage.Table;

    public class Utility
    {
        private CloudTable settingsTable;
        private AzureTableProvider azureTableProvider;

        public Utility()
        {
            azureTableProvider = new AzureTableProvider(Settings.StorageAccountConnectionString);
            settingsTable = azureTableProvider.GetAzureTableReference(Settings.ConfigurationSettingsTableName);
        }

        public async Task<string> ReadConfigurationSettingsFromTable(string settingName)
        {
            return (await azureTableProvider.GetEntityAsync<ConfigurationSettingsEntity>(settingsTable, "ConfigurationSetting", settingName))?.settingValue;
        }
    }
}
