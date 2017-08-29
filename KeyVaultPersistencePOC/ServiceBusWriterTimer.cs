using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System.Runtime.Caching;

namespace KeyVaultPersistencePOC
{
    public static class ServiceBusWriterTimer
    {

        // cache
        static MemoryCache memoryCache = MemoryCache.Default;

        [FunctionName("ServiceBusWriterTimer")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            /** Service Bus Connection Strings **/
            var serviceBusEndpoint = "Endpoint=sb://s00197svb0platmsgdev0.servicebus.windows.net/";
            var sendPolicyName = "activetestqueuemanage";
            var entityPath = "active-test-queue";

            /** Attempt to gather secrets from cache **/
            var cacheObject1 = memoryCache["ServiceBusSendPrimaryKey"];
            var primaryKey = (cacheObject1 == null) ? "invalid" : (string) cacheObject1;

            var cacheObject2 = memoryCache["ServiceBusSendSecondaryKey"];
            var secondaryKey = (cacheObject2 == null) ? "invalid" : (string)cacheObject2;

            /** Service Bus Connection Config **/
            var primaryWriterConfig = new ServiceBusWriterConfig(new NullEncryptor(), new NullCompressor(),
                serviceBusPrimaryKey)
            {
                SendPolicyName = sendPolicyName,
                ServiceBusEndpoint = serviceBusEndpoint,
                UseCompression = false,
                UseEncryption = false,
                EntityPath = entityPath
            };
            var secondaryWriterConfig = new ServiceBusWriterConfig(new NullEncryptor(), new NullCompressor(),
                serviceBusSecondaryKey, ConnectionType.Secondary)
            {
                SendPolicyName = sendPolicyName,
                ServiceBusEndpoint = serviceBusEndpoint,
                UseCompression = false,
                UseEncryption = false,
                EntityPath = entityPath
            };

            /** Send message **/
            var sender = new WriteServiceBusQueueCommand(primaryWriterConfig, secondaryWriterConfig);
            var result = await sender.WriteMessageAsync("This is a simple message to the Service Bus Queue!", new Dictionary<string, object>());



        }
    }
}
