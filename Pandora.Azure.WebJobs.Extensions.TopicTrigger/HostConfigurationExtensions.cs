// Copyright (c) PandoraJewelry. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Config;
using Pandora.Azure.WebJobs.Extensions.TopicTrigger.Binding;
using System;

namespace Pandora.Azure.WebJobs.Extensions.TopicTrigger
{
    public static class TopicHostConfigurationExtensions
    {
        public static void UseTopicTriggers(this JobHostConfiguration config)
        {
            UseTopicTriggers(config, new TopicConfiguration());
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