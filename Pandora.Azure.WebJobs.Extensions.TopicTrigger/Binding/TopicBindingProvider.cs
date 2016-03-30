// Copyright (c) PandoraJewelry. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs.Host.Triggers;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Pandora.Azure.WebJobs.Extensions.TopicTrigger.Binding
{
    internal class TopicBindingProvider : ITriggerBindingProvider
    {
        #region fields
        private readonly TopicConfiguration _topicConfig;
        #endregion

        #region constructors
        public TopicBindingProvider(TopicConfiguration topicConfig)
        {
            if (topicConfig == null)
                throw new ArgumentNullException(nameof(topicConfig));

            _topicConfig = topicConfig;
        }
        #endregion

        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            if (context == null)            
                throw new ArgumentNullException(nameof(context));            

            ParameterInfo parameter = context.Parameter;
            TopicTriggerAttribute attribute = parameter.GetCustomAttribute<TopicTriggerAttribute>(inherit: false);
            if (attribute == null)            
                return Task.FromResult<ITriggerBinding>(null);            

            var binding = new TopicBinding(attribute.TopicName, _topicConfig, context.Parameter);
            return Task.FromResult<ITriggerBinding>(binding);
        }
    }
}