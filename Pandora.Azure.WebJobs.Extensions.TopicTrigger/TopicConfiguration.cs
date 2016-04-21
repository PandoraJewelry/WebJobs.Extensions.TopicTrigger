// Copyright (c) PandoraJewelry. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.ServiceBus.Messaging;
using Pandora.Azure.WebJobs.Extensions.TopicTrigger.Filters;
using Pandora.Azure.WebJobs.PipelineCore;

namespace Pandora.Azure.WebJobs.Extensions.TopicTrigger
{
    public class TopicConfiguration
    {
        #region fields
        public OnMessageOptions MessageOptions { get; set; }
        public ISubscriptionFilter Filter { get; set; }
        public PipelineProcessor Processor { get; set; }
        public int ReScanInterval { get; set; }
        #endregion

        #region constructors
        public TopicConfiguration()
        {
            MessageOptions = new OnMessageOptions()
            {
                MaxConcurrentCalls = 1,
                AutoComplete = true
            };
            Filter = new EverySubscriptionFilter();
            Processor = new PipelineProcessor(MessageOptions);
            ReScanInterval = 60 * 1000;
        }
        #endregion
    }
}
