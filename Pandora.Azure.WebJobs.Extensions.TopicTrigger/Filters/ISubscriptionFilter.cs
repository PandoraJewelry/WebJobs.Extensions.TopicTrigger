// Copyright (c) PandoraJewelry. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Pandora.Azure.WebJobs.Extensions.TopicTrigger.Filters
{
    public interface ISubscriptionFilter
    {
        bool Watch(string topicName, string subscriptionPath);
    }
}
