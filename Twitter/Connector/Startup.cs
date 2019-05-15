// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Owin;
using System.Web.Http;
using Microsoft.Owin.Security.ActiveDirectory;
using Microsoft.Owin;

[assembly: OwinStartup(typeof(Sample.TwitterConnector.Startup))]
namespace Sample.TwitterConnector
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();
            ConfigureOAuth(app);
            app.UseWebApi(config);
        }

        private void ConfigureOAuth(IAppBuilder app)
        {
            var audience = Connector.Settings.AADAppUri;
            var metaDataAddress = "https://login.microsoftonline.com/common/federationmetadata/2007-06/federationmetadata.xml";
            
            app.UseWindowsAzureActiveDirectoryBearerAuthentication(
                new WindowsAzureActiveDirectoryBearerAuthenticationOptions
                {
                    MetadataAddress = metaDataAddress,
                    TokenValidationParameters = new System.IdentityModel.Tokens.TokenValidationParameters() { ValidateIssuer = false, ValidAudience = audience }
                });
        }
    }
}