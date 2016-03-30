// Copyright (c) PandoraJewelry. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.ServiceBus.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace Pandora.Azure.WebJobs.Extensions.TopicTrigger.Processors
{
    public interface IMessageProcessor
    {
        Task<bool> BeginProcessingMessageAsync(BrokeredMessage message, CancellationToken cancellationToken);
        Task CompleteProcessingMessageAsync(BrokeredMessage message, FunctionResult result, CancellationToken cancellationToken);
    }
}
