using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System.Runtime.Caching;
using Starbucks.Dp.Platform.ServiceBusWriter;
using Starbucks.Dp.Platform.Shared;
using Starbucks.Dp.Platform.Shared.CircuitBreaker;
using System.Collections.Generic;

namespace KeyVaultPersistencePOC
{
    public static class ServiceBusWriterTimer
    {

        // cache
        static MemoryCache memoryCache = MemoryCache.Default;

        [FunctionName("ServiceBusWriterTimer")]
        public static async void Run([TimerTrigger("0 */2 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"ServiceBusWriterTimer function executed at: {DateTime.Now}");

            /** Service Bus Connection Strings **/
            var serviceBusEndpoint = "Endpoint=sb://s00197svb0platmsgdev0.servicebus.windows.net/";
            var sendPolicyName = "activetestqueuemanage";
            var entityPath = "active-test-queue";

            /** Attempt to gather secrets from cache **/
            log.Info("Reading 'ServiceBusSendPrimaryKey' from cache..");
            var cacheObject1 = memoryCache["ServiceBusSendPrimaryKey"];
            var primaryKey = (cacheObject1 == null) ? null : (string) cacheObject1;

            log.Info("Reading 'ServiceBusSendSecondaryKey' from cache..");
            var cacheObject2 = memoryCache["ServiceBusSendSecondaryKey"];
            var secondaryKey = (cacheObject2 == null) ? null : (string)cacheObject2;

            /** Handle case that cached objects returned null **/
            if (primaryKey == null || secondaryKey == null)
            {
                /** Do something here to handle cache failure **/
                log.Warning("Secret key values return null..exiting function");
                return; 
            }

            /** Service Bus Connection Config **/
            log.Info("Setting up ServiceBusWriterConfig objects..");
            var primaryWriterConfig = new ServiceBusWriterConfig(new NullEncryptor(), new NullCompressor(),
                primaryKey)
            {
                SendPolicyName = sendPolicyName,
                ServiceBusEndpoint = serviceBusEndpoint,
                UseCompression = false,
                UseEncryption = false,
                EntityPath = entityPath
            };
            var secondaryWriterConfig = new ServiceBusWriterConfig(new NullEncryptor(), new NullCompressor(),
                secondaryKey, ConnectionType.Secondary)
            {
                SendPolicyName = sendPolicyName,
                ServiceBusEndpoint = serviceBusEndpoint,
                UseCompression = false,
                UseEncryption = false,
                EntityPath = entityPath
            };

            /** Send message **/
            log.Info("Sending message to Service Bus Queue..");
            var sender = new WriteServiceBusQueueCommand(primaryWriterConfig, secondaryWriterConfig);
            var result = await sender.WriteMessageAsync("This is a simple message to the Service Bus Queue!", new Dictionary<string, object>());

            log.Info("Messages sent");
        }
    }
}
