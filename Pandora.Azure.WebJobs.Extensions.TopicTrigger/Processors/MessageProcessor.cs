// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
// A large part of this class was copied straight from MS (github.com/Azure/azure-webjobs-sdk).

using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace Pandora.Azure.WebJobs.Extensions.TopicTrigger.Processors
{
    public class MessageProcessor: IMessageProcessor
    {
        private OnMessageOptions _messageOptions;

        public MessageProcessor(OnMessageOptions messageOptions)
        {
            if (messageOptions == null)            
                throw new ArgumentNullException(nameof(messageOptions));

            _messageOptions = messageOptions;
        }

        public virtual Task<bool> BeginProcessingMessageAsync(BrokeredMessage message, CancellationToken cancellationToken)
        {
            return Task.FromResult<bool>(true);
        }
        public virtual async Task CompleteProcessingMessageAsync(BrokeredMessage message, FunctionResult result, CancellationToken cancellationToken)
        {
            if (result.Succeeded)
            {
                if (!_messageOptions.AutoComplete)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await message.CompleteAsync();
                }
            }
            else
            {
                cancellationToken.ThrowIfCancellationRequested();
                await message.AbandonAsync();
            }
        }
    }
}
