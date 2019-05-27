// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.TwitterConnector
{
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Web.Http;
    using System.Web.Http.Routing;
    using Microsoft.WindowsAzure.Storage.Table;
    using System.Threading.Tasks;
    using Connector;
    using Sample.TwitterSDK;

    public class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            var routes = config.Routes;

            routes.MapHttpRoute("DefaultApiWithId", "Api/{controller}/{id}", new { id = RouteParameter.Optional });
            routes.MapHttpRoute("DefaultApiGet", "Api/{controller}", new { action = "Get" }, new { httpMethod = new HttpMethodConstraint(HttpMethod.Get) });
            routes.MapHttpRoute("DefaultApiPost", "Api/{controller}", new { action = "Post" }, new { httpMethod = new HttpMethodConstraint(HttpMethod.Post) });

            GlobalConfiguration.Configuration.Formatters.Clear();
            GlobalConfiguration.Configuration.Formatters.Add(new JsonMediaTypeFormatter());

            config.Filters.Add(new ExceptionFilter());

            InitializeStorageEntities().Wait();
        }

        private static async Task InitializeStorageEntities()
        {
            AzureTableProvider azureTableProvider = new AzureTableProvider(Settings.StorageAccountConnectionString);
            CloudTable tokenTable = await azureTableProvider.EnsureTableExistAsync(Settings.TokenTableName);
            CloudTable pageJobMappingTable = await azureTableProvider.EnsureTableExistAsync(Settings.PageJobMappingTableName);
            CloudTable settingsTable = await azureTableProvider.EnsureTableExistAsync(Settings.ConfigurationSettingsTableName);
            await GetConfigurationSettingFomStorge();
        }

        private static async Task GetConfigurationSettingFomStorge()
        {
            Utility utility = new Utility();
            Settings.AAdAppId = await utility.ReadConfigurationSettingsFromTable("AAdAppId");
            Settings.AAdAppSecret = await utility.ReadConfigurationSettingsFromTable("AAdAppSecret");
            SettingsTwitter.TwitterApiKey = await utility.ReadConfigurationSettingsFromTable("TwitterApiKey");
            SettingsTwitter.TwitterApiSecretKey = await utility.ReadConfigurationSettingsFromTable("TwitterApiSecretKey");
            SettingsTwitter.TwitterAccessToken = await utility.ReadConfigurationSettingsFromTable("TwitterAccessToken");
            SettingsTwitter.TwitterAccessTokenSecret = await utility.ReadConfigurationSettingsFromTable("TwitterAccessTokenSecret");
            Settings.AADAppUri = await utility.ReadConfigurationSettingsFromTable("AADAppUri");
            Settings.APPINSIGHTS_INSTRUMENTATIONKEY = await utility.ReadConfigurationSettingsFromTable("APPINSIGHTS_INSTRUMENTATIONKEY");
            if (Settings.APPINSIGHTS_INSTRUMENTATIONKEY == null)
            {
                Settings.APPINSIGHTS_INSTRUMENTATIONKEY = string.Empty;
            }
        }
    }
}