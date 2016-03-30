// Copyright (c) PandoraJewelry. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Host.Triggers;
using System;

namespace Pandora.Azure.WebJobs.Extensions.TopicTrigger.Binding
{
    internal class TopicExtensionConfig : IExtensionConfigProvider
    {
        #region fields
        private TopicConfiguration _topicConfig;
        #endregion

        #region constructors
        public TopicExtensionConfig(TopicConfiguration topicConfig)
        {
            if (topicConfig == null)
                throw new ArgumentNullException(nameof(topicConfig));

            _topicConfig = topicConfig;
        } 
        #endregion

        public void Initialize(ExtensionConfigContext context)
        {
            if (context == null)            
                throw new ArgumentNullException(nameof(context));

            IExtensionRegistry extensions = context.Config.GetService<IExtensionRegistry>();

            var triggerprovider = new TopicBindingProvider(_topicConfig);
            extensions.RegisterExtension<ITriggerBindingProvider>(triggerprovider);
        }
    }
}
