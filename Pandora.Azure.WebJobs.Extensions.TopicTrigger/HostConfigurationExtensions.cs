// Copyright (c) PandoraJewelry. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;
using Pandora.Azure.WebJobs.Extensions.TopicTrigger.Processors;
using Pandora.Azure.WebJobs.Extensions.TopicTrigger.Filters;
using Pandora.Azure.WebJobs.Extensions.TopicTrigger.Binding;

namespace Pandora.Azure.WebJobs.Extensions.TopicTrigger
{
    public static class TopicHostConfigurationExtensions
    {
        public static void UseTopicTriggers(this JobHostConfiguration config)
        {
            var options = new OnMessageOptions()
            {
                MaxConcurrentCalls = 1,
                AutoComplete = true
            };
            var topicconfig = new TopicConfiguration()
            {
                Processor = new MessageProcessor(options),
                Filter = new EverySubscriptionFilter(),
                ReScanInterval = 60*1000,
                MessageOptions = options
            };

            UseTopicTriggers(config, topicconfig);
        }
        public static void UseTopicTriggers(this JobHostConfiguration config, TopicConfiguration topicConfig)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (topicConfig == null)
                throw new ArgumentNullException(nameof(topicConfig));

            var extensionConfig = new TopicExtensionConfig(topicConfig);
            var extensions = config.GetService<IExtensionRegistry>();
            extensions.RegisterExtension<IExtensionConfigProvider>(extensionConfig);
        }
    }
}