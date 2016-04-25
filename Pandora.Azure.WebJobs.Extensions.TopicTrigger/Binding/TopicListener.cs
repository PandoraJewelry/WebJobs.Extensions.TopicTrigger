// Copyright (c) PandoraJewelry. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pandora.Azure.WebJobs.Extensions.TopicTrigger.Binding
{
    internal class TopicListener : IListener
    {
        #region fields
        private static readonly TraceSource _trace = new TraceSource(Consts.TraceName, SourceLevels.Error);
        private readonly TopicConfiguration _topicConfig;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ITriggeredFunctionExecutor _executor;
        private readonly string _topicName;
        private readonly Dictionary<string, MessageReceiver> _listeners;
        private MessagingFactory _factory;
        private NamespaceManager _manager;
        private Task _timer;
        private bool _disposed;
        #endregion

        #region constructors
        public TopicListener(string topicName, TopicConfiguration topicConfig, ITriggeredFunctionExecutor executor)
        {
            if (topicName == null)
                throw new ArgumentNullException(nameof(topicName));
            if (topicConfig == null)
                throw new ArgumentNullException(nameof(topicConfig));
            if (executor == null)
                throw new ArgumentNullException(nameof(executor));

            _cancellationTokenSource = new CancellationTokenSource();
            _listeners = new Dictionary<string, MessageReceiver>();
            _topicName = topicName;
            _executor = executor;
            _topicConfig = topicConfig;
            _timer = new Task(async () => await Timer(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
        }
        #endregion

        #region lifecycle
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            if (_factory != null)
                throw new InvalidOperationException("The listener has already been started.");

            cancellationToken.ThrowIfCancellationRequested();

            var connectionString = AmbientConnectionStringProvider.Instance.GetConnectionString(ConnectionStringNames.ServiceBus);

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ApplicationException("Missing config for ServiceBus");

            _factory = MessagingFactory.CreateFromConnectionString(connectionString);
            _manager = NamespaceManager.CreateFromConnectionString(connectionString);

            await ScanAndUpdateForSubscriptionsAsync();
            _timer.Start();
        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            if (_factory == null)
                throw new InvalidOperationException("The listener has not yet been started or has already been stopped.");

            _cancellationTokenSource.Cancel();
            await _timer;
            await Task.WhenAll(_listeners.Select(p => p.Value.CloseAsync()));

            _listeners.Clear();
            _factory = null;
        }
        public void Dispose()
        {
            if (!_disposed)
            {
                // Running callers might still be using the cancellation token.
                // Mark it canceled but don't dispose of the source while the callers are running.
                // Otherwise, callers would receive ObjectDisposedException when calling token.Register.
                // For now, rely on finalization to clean up _cancellationTokenSource's wait handle (if allocated).
                _cancellationTokenSource.Cancel();
                _timer.Dispose();

                _disposed = true;
            }
        }
        public void Cancel()
        {
            ThrowIfDisposed();
            _cancellationTokenSource.Cancel();
        }
        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(this.GetType().Name);
        }
        #endregion

        #region timer
        private async Task Timer(CancellationToken token)
        {
            while (true)
            {
                try
                {
                    await Task.Delay(_topicConfig.ReScanInterval, token);
                    await ScanAndUpdateForSubscriptionsAsync();
                }
                catch (TaskCanceledException) { }

                if (token.IsCancellationRequested)
                    break;
            }
        }
        private async Task ScanAndUpdateForSubscriptionsAsync()
        {
            var v1 = await _manager.GetSubscriptionsAsync(_topicName);
            var v2 = v1.Where(p => p.Status == EntityStatus.Active);
            var v3 = v2.Where(p => _topicConfig.Filter.Watch(_topicName, p.Name));
            var watchme = new HashSet<string>(v3.Select(sub => sub.Name));

            int adding = 0;
            var addme = watchme.Where(name => !_listeners.ContainsKey(name));
            foreach (var name in addme)
            {
                var entityPath = string.Format("{0}/subscriptions/{1}", _topicName, name);
                var listener = await _factory.CreateMessageReceiverAsync(entityPath);
                listener.OnMessageAsync(msg => ProcessMessageAsync(name, msg), _topicConfig.MessageOptions);
                _listeners[name] = listener;
                adding++;
            }

            var removeme = _listeners.Where(kvp => !watchme.Contains(kvp.Key)).ToList();
            await Task.WhenAll(removeme.Select(p => p.Value.CloseAsync()));
            foreach (var kvp in removeme)
                _listeners.Remove(kvp.Key);

            _trace.TraceEvent(TraceEventType.Information, 1, "Monitoring subscriptions: Adding({0}), Removing({1}), Total({2})", adding, removeme.Count, _listeners.Count);
        }
        #endregion

        #region process
        private async Task ProcessMessageAsync(string subscriptionName, BrokeredMessage message)
        {
            var token = _cancellationTokenSource.Token;
            var processor = _topicConfig.Processor;
            Action<string> log = msg => _trace.TraceEvent(TraceEventType.Verbose, 2, "Message {0}/{1}/{2} - {3}", _topicName, subscriptionName, message.MessageId, msg);

            log("Found");
            if (await processor.BeginProcessingMessageAsync(message, token))
            {
                log("Processing");
                var input = new TriggeredFunctionData
                {
                    TriggerValue = message
                };

                var result = await _executor.TryExecuteAsync(input, _cancellationTokenSource.Token);
                log(result.Succeeded ? "Success" : string.Format("Failed ({0})", result.Exception));

                await processor.CompleteProcessingMessageAsync(message, result, token);
            }
            else
                log("Skiping");
        }
        #endregion
    }
}
