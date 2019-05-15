// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.TwitterConnector
{
    using Microsoft.ApplicationInsights.Extensibility;
    using System.Web.Http;
    using System.Web.Routing;
    using System.Web.Optimization;

    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            TelemetryConfiguration.Active.InstrumentationKey = Connector.Settings.APPINSIGHTS_INSTRUMENTATIONKEY;
        }
    }
}
