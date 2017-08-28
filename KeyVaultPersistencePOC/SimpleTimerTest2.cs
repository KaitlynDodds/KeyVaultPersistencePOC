using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System.Runtime.Caching;

namespace KeyVaultPersistencePOC
{
    public static class SimpleTimerTest2
    {
        static MemoryCache memoryCache = MemoryCache.Default;

        [FunctionName("TimerTrigger2")]
        public static void Run([TimerTrigger("0 */2 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"SimpleTimerTest2 executed at: {DateTime.Now}");

            var cacheObject = memoryCache["cachedCount"];
            var cachedCount = (cacheObject == null) ? 0 : (int)cacheObject;
            memoryCache.Set("cachedCount", ++cachedCount, DateTimeOffset.Now.AddMinutes(20));

            log.Info($"TimerTest2 triggered memory count {cachedCount}");
        }
    }
}