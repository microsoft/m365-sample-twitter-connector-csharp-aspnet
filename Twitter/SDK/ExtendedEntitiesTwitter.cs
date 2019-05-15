// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.TwitterSDK
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Used with more than one photos, Video, GIF is included
    /// </summary>
    public class ExtendedEntitiesTwitter
    {
        /// <summary>
        /// Collection of Media objects
        /// </summary>
        [JsonProperty("media")]
        public List<ExtendedMediaTwitter> ExtendedMediaObjects { get; set; }
    }
}