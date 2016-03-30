// Copyright (c) PandoraJewelry. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Pandora.Azure.WebJobs.Extensions.TopicTrigger.Binding
{
    internal class TopicBinding : ITriggerBinding
    {
        #region fields
        private readonly TopicConfiguration _topicConfig;
        private readonly ParameterInfo _parameter;
        private readonly IReadOnlyDictionary<string, Type> _bindingContract;
        private readonly string _topicName;
        #endregion

        #region constructors
        public TopicBinding(string topicName, TopicConfiguration topicConfig, ParameterInfo parameter)
        {
            if (topicName == null)
                throw new ArgumentNullException(nameof(topicName));
            if (topicConfig == null)
                throw new ArgumentNullException(nameof(topicConfig));
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            _topicName = topicName;
            _parameter = parameter;
            _topicConfig = topicConfig;
            _bindingContract = CreateBindingDataContract();
        }
        #endregion

        public IReadOnlyDictionary<string, Type> BindingDataContract
        {
            get { return _bindingContract; }
        }

        public Type TriggerValueType
        {
            get { return typeof(BrokeredMessage); }
        }

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            BrokeredMessage message = value as BrokeredMessage;

            IValueBinder valueBinder = new TopicValueBinder(message, _parameter);
            return Task.FromResult<ITriggerData>(new TriggerData(valueBinder, GetBindingData(message)));
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            var listener = new TopicListener(_topicName, _topicConfig, context.Executor);
            return Task.FromResult<IListener>(listener);
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new TopicParameterDescriptor
            {
                TopicName = _topicName,
                Name = _parameter.Name
            };
        }

        private IReadOnlyDictionary<string, object> GetBindingData(BrokeredMessage value)
        {
            /// TODO: I dont know the real value of this. Look into it.
            Dictionary<string, object> bindingData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            bindingData.Add("TopicTrigger", value);
            return bindingData;
        }

        private IReadOnlyDictionary<string, Type> CreateBindingDataContract()
        {
            /// TODO: I dont know the real value of this. Look into it.
            Dictionary<string, Type> contract = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            contract.Add("TopicTrigger", typeof(BrokeredMessage));
            return contract;
        }
    }
}
