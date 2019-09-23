// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.QueueAdapter.Interface
{
    public interface IQueueSerializer
    {
        string Serialize<T>(T item);

        T Deserialize<T>(string item);
    }
}