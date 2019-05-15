// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.TwitterSDK
{
    using Newtonsoft.Json;

    public class AccountTwitter
    {
        [JsonProperty("id_str")]
        public string Id { get; set; }

        [JsonProperty("screen_name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string EmaildId { get; set; }
    }
}
