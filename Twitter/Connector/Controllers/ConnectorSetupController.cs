// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.TwitterConnector
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Linq.Expressions;
    using Microsoft.WindowsAzure.Storage.Table;
    using Connector;
    using System.Net;
    using Sample.TwitterSDK;

    /// <summary>
    /// API controller for all native connector setups
    /// </summary>
    [ApiAuthorizationModule]
    public class ConnectorSetupController : ApiController
    {

        public ConnectorSetupController()
        {
        }

        /// <summary>
        /// Validate Connector Setup 
        /// </summary>
        /// <returns>true for validation success</returns>
        [HttpGet]
        [Route("setup/Validate")]
        public Task<HttpResponseMessage> ValidateSetup()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            var configuration = new HttpConfiguration();
            request.SetConfiguration(configuration);
            return Task.FromResult(request.CreateResponse<ValidationResponse>(HttpStatusCode.OK, new ValidationResponse(true)));
        }
    }
}