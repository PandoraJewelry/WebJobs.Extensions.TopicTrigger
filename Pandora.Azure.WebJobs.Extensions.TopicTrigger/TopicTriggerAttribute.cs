// Copyright (c) PandoraJewelry. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;

namespace Pandora.Azure.WebJobs.Extensions.TopicTrigger
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class TopicTriggerAttribute : Attribute
    {
        public TopicTriggerAttribute(string topicName)
        {
            TopicName = topicName;
        }
        public string TopicName { get; private set; }
    }
}