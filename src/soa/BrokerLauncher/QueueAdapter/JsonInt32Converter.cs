// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.BrokerLauncher.QueueAdapter
{
    using System;

    using Newtonsoft.Json;

    public class Int64ToInt32Converter : JsonConverter
    {
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) =>
            (reader.TokenType == JsonToken.Integer) ? Convert.ToInt32(reader.Value) : serializer.Deserialize(reader);

        public override bool CanConvert(Type objectType) => objectType == typeof(Int32) || objectType == typeof(Int64) || objectType == typeof(int) || objectType == typeof(object);
    }
}