// Copyright (c) PandoraJewelry. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs.Extensions.Bindings;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

namespace Pandora.Azure.WebJobs.Extensions.TopicTrigger.Binding
{
    internal class TopicValueBinder : ValueBinder
    {
        #region fields
        private const string JsonContentType = "application/json";
        private const string PlainTextType = "text/plain";
        private readonly BrokeredMessage _value; 
        #endregion

        public TopicValueBinder(BrokeredMessage value, ParameterInfo parameter)
            : base(parameter.ParameterType)
        {
            _value = value;
        }

        public override object GetValue()
        {
            if (Type == typeof(BrokeredMessage))
                return _value;
            else if (Type == typeof(Stream))
                return _value.GetBody<Stream>();
            else
            {
                using (var stream = _value.GetBody<Stream>())
                {
                    if ((_value.ContentType == JsonContentType) || (_value.ContentType == PlainTextType))
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            var body = reader.ReadToEnd();
                            return JsonConvert.DeserializeObject(body, Type);
                        }
                    }
                    else
                    {
                        var serializer = new DataContractSerializer(Type);
                        return serializer.ReadObject(stream);
                    }
                }
            }
        }

        public override string ToInvokeString()
        {
            /// TODO: what is this used for
            return "TopicTrigger";
        }
    }
}
