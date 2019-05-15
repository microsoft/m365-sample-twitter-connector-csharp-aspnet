// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.TwitterSDK
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Helper for list of errors returned by Twitter
    /// </summary>
    public class ErrorsTwitter
    {
        /// <summary>
        /// Errors returned from twitter
        /// </summary>
        [JsonProperty("errors")]
        public IList<ErrorTypeTwitter> Errors { get; set; }
    }
}
