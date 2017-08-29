using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Starbucks.Dp.Platform.Security.Azure.KeyVault;
using System;
using System.Runtime.Caching;
using System.Collections.Generic;

namespace KeyVaultPersistencePOC
{
    public static class KeyVaultCaching
    {
        /// <summary>
        /// Application id  =>  Service Principal (GUID)
        /// </summary>
        /// <remarks>
        /// TODO : Change the following to your Service Principal GUID
        /// </remarks>
        private const string ApplicationId = "48cad164-3cf8-4a76-bedc-e009030ade61";

        /// <summary>
        /// Application key =>  Service Principal Key
        /// </summary>
        /// <remarks>
        /// TODO : Change the following to your Service Principal Key
        /// </remarks>
        private const string ApplicationKey = "amtkaGZkZm5qZGZrZGFmbGRrbWZuZHNrYmpjY2FkbGtqMzMyZA==";

        /// <summary>
        /// Key Vault URI   =>  Azure Key Vault URI
        /// </summary>
        /// <remarks>
        /// TODO : Change the following to your Azure Key Vault URI
        /// </remarks>
        private const string KeyVaultUri = "https://s00197kvt0platmsgdev0.vault.azure.net/";

        /// <summary>
        /// Key Vault secret name   =>  Secret name to be read
        /// </summary>
        /// <remarks>
        /// TODO : Change the following to your Secret Name
        /// </remarks>
        private const string SecretName = "AzureKeyVaultExampleSecretValue";

        /// <summary>
        /// Key Vault key name
        /// </summary>
        /// <remarks>
        /// TODO : Change the following to your Key Name
        /// </remarks>
        private const string KeyName = "EncryptionKey";

        // cache
        static MemoryCache memoryCache = MemoryCache.Default;

        [FunctionName("KeyVaultCaching")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("KeyVaultConnectionSetup function processed a request.");

            /** Secret values to fetch and cache **/
            var secretNames = new HashSet<string> { "ServiceBusSendPrimaryKey", "ServiceBusSendSecondaryKey" };

            /** Get Secret values from Key Vault **/
            log.Info("\nFetching secrets from Azure Key Vault..\n");
            var keyVaultSecrets = await KeyVaultManager.Instance.GetCurrentSecretValuesAsync(ApplicationId, ApplicationKey, KeyVaultUri, secretNames);

            /** cache each secret value **/
            log.Info("Caching secrets..");
            foreach (string secretName in secretNames)
            {
                log.Info($"Caching {secretName}...");
                var secretValue = keyVaultSecrets[secretName];
                memoryCache.Set(secretName, secretValue, DateTimeOffset.Now.AddMinutes(20));
            }
            log.Info("Cache complete\n");

            return req.CreateResponse(HttpStatusCode.OK, "Hello, world");
        }
    }
}
