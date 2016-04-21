# Azure Webjobs SDK Extension for Azure ServiceBus

## Introduction
This extension will enable recept of all messages on every subscription in a single **Topic** instead of needing to know the subscription name ahead of time like with `[ServiceBusTrigger]`

Once referenced you can enable it on the `JobHostConfiguration` object.

    var config = new JobHostConfiguration()
    config.UseTopicTriggers();
    var host = new JobHost(config);
    host.RunAndBlock();

It supports
  1. JSON serialized POCO objects
  2. DataContractSerializer POCO objects
  3. BrokeredMessages
  4. Streams

And decorate each function like this:

    public static void MyFunction([TopicTrigger("my_topic")] POCO messages)

This way you will only need to implement **one function** to receive all subscriptions messages, instead of one function per subscription as is the case with the `[ServiceBusTrigger]`.

## Our use case
We use [Azure Service Bus](5) to load balance the processing of [Microsoft Dynamics CRM](2) [data](3) into an [Azure SQL DB](4). Each subscription represents a single logical entity inside of a [remoteexecutioncontext](1). Each entity can be independently processed, but **must** be processed in order.

##Tracing
Tracing can be turned on by adding in

	<switches>
	  <add name="Pandora.Azure.WebJobs.Extensions.TopicTrigger" value="Verbose" />
	</switches>
	<sources>
	  <source name="Pandora.Azure.WebJobs.Extensions.TopicTrigger" />
	</sources>

## Installation
You can obtain it [through Nuget](https://www.nuget.org/packages/Pandora.Azure.WebJobs.Extensions.TopicTrigger/) with:

    Install-Package Pandora.Azure.WebJobs.Extensions.TopicTrigger

Or **clone** this repo and reference it.

## Refrences
  1. https://github.com/Azure/azure-webjobs-sdk
  2. https://github.com/Azure/azure-webjobs-sdk-extensions
  3. https://github.com/ealsur/WebJobs.Extensions.GroupQueueTrigger
[1]: https://msdn.microsoft.com/en-us/library/microsoft.xrm.sdk.remoteexecutioncontext.aspx
[2]: https://www.microsoft.com/en-us/dynamics/crm.aspx
[3]: https://msdn.microsoft.com/en-us/library/gg309677.aspx
[4]: https://azure.microsoft.com/en-us/documentation/services/sql-database/
[5]: https://azure.microsoft.com/en-us/documentation/services/service-bus/
