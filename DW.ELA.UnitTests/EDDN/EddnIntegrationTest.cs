﻿namespace DW.ELA.UnitTests.EDDN
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DW.ELA.Plugin.EDDN;
    using DW.ELA.Plugin.EDDN.Model;
    using DW.ELA.UnitTests.Utility;
    using DW.ELA.Utility;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;

    public class EddnIntegrationTest
    {
        [Test]
        [Explicit]
        public async Task IntegrationTestUploadToEddn()
        {
            var restClient = new ThrottlingRestClient.Factory().CreateRestClient("https://eddn.edcd.io:4430/upload/");
            var facade = new EddnApiFacade(restClient);
            var @event = new TestEddnEvent();
            @event.Header = new Dictionary<string, string>
            {
                { "uploaderID", TestCredentials.UserName },
                { "softwareName", AppInfo.Name },
                { "softwareVersion", AppInfo.Version }
            };
            var message = @event.Message;
            message.Add("timestamp", DateTime.UtcNow);
            message.Add("event", "FSDJump");
            message.Add("StarSystem", "Sol");
            message.Add("SystemAddress", 12345678901234567890);
            message.Add("StarPos", new JArray(new[] { 0, 0, 0 }));

            Assert.IsTrue(new EventSchemaValidator().ValidateSchema(@event));
            await facade.PostEventAsync(@event);
        }

        private class TestEddnEvent : EddnEvent
        {
            [JsonProperty("message")]
            public JObject Message { get; } = new JObject();

            public override string SchemaRef => "https://eddn.edcd.io/schemas/journal/1/test";
        }
    }
}
