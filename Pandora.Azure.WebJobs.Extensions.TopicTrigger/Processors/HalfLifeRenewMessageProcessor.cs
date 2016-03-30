// Copyright (c) PandoraJewelry. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pandora.Azure.WebJobs.Extensions.TopicTrigger.Processors
{
    public class HalfLifeRenewMessageProcessor : MessageProcessor, IMessageProcessor
    {
        #region fields
        private const string _renewToken = "RenewToken"; 
        #endregion

        #region constructors
        public HalfLifeRenewMessageProcessor(OnMessageOptions messageOptions) : base(messageOptions) { } 
        #endregion

        public override Task<bool> BeginProcessingMessageAsync(BrokeredMessage message, CancellationToken cancellationToken)
        {
            /// start auto renewing the message. useful when processing takes more than max service bus lock (5 min)
            message.Properties[_renewToken] = AutoRenew(message);

            return base.BeginProcessingMessageAsync(message, cancellationToken);
        }
        public override Task CompleteProcessingMessageAsync(BrokeredMessage message, FunctionResult result, CancellationToken cancellationToken)
        {
            /// stop the auto renew
            (message.Properties[_renewToken] as Action)();

            return base.CompleteProcessingMessageAsync(message, result, cancellationToken);
        }

        #region tools
        private static Action AutoRenew(BrokeredMessage message)
        {
            var halflife = (int)((message.LockedUntilUtc - DateTime.Now.ToUniversalTime()).TotalMilliseconds / 2);
            var token = new CancellationTokenSource();
            var id = message.MessageId;

            var task = Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await Task.Delay(halflife, token.Token);
                        await message.RenewLockAsync();
                    }
                    catch (TaskCanceledException) { }
                    if (token.IsCancellationRequested)
                        break;
                }
            }, token.Token);

            return () =>
            {
                token.Cancel();
                task.Wait();
            };
        } 
        #endregion
    }
}
