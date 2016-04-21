// Copyright (c) PandoraJewelry. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.ServiceBus.Messaging;
using Pandora.Azure.WebJobs.Extensions.TopicTrigger.Filters;
using Pandora.Azure.WebJobs.Extensions.TopicTrigger.Processors;

namespace Pandora.Azure.WebJobs.Extensions.TopicTrigger
{
    public class TopicConfiguration
    {
        public OnMessageOptions MessageOptions { get; set; }
        public ISubscriptionFilter Filter { get; set; }
        public IMessageProcessor Processor { get; set; }
        public int ReScanInterval { get; set; }

        public TopicConfiguration()
        {
            MessageOptions = new OnMessageOptions();
            Filter = new EverySubscriptionFilter();
            Processor = new MessageProcessor(MessageOptions);
            ReScanInterval = 1;
        }
    }
}
