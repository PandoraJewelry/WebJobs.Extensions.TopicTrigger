// Copyright (c) PandoraJewelry. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs.Host.Protocols;
using System.Collections.Generic;
using System.Globalization;

namespace Pandora.Azure.WebJobs.Extensions.TopicTrigger.Binding
{
    public class TopicParameterDescriptor : TriggerParameterDescriptor
    {
        public string TopicName { get; set; }

        public override string GetTriggerReason(IDictionary<string, string> arguments)
        {
            return string.Format(CultureInfo.CurrentCulture, "New ServiceBus message detected in '{0}'.", TopicName);
        }
    }
}
