// Copyright (c) PandoraJewelry. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.


using Pandora.Azure.WebJobs.PipelineCore;
using Pandora.ServiceBus;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pandora.Azure.WebJobs.Extensions.TopicTrigger.Processors
{
    public class HalfLifeRenewMessageProcessor : IMessageProcessor
    {
        public async Task Invoke(IPipelineContext context, Func<Task> next, CancellationToken cancellationToken)
        {
            var done = context.Message.AutoRenew();
            try
            {
                await next();
            }
            finally
            {
                done();
            }
        }
    }
}
