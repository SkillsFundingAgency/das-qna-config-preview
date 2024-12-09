using Azure;
using Azure.Data.Tables;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.QnA.Config.Preview.Settings
{
    public static class ConfigurationService
    {
        public static async Task<IWebConfiguration> GetConfig(string environment, string storageConnectionString, string version, string serviceName)
        {
            if (environment == null) throw new ArgumentNullException(nameof(environment));
            if (storageConnectionString == null) throw new ArgumentNullException(nameof(storageConnectionString));

            try
            {
                var tableClient = new TableClient(storageConnectionString, "Configuration");

                var entity = await tableClient.GetEntityAsync<TableEntity>(environment, $"{serviceName}_{version}");
                var dataString = entity.Value.GetString("Data");

                if (string.IsNullOrEmpty(dataString))
                    throw new Exception("The 'Data' property is missing or empty.");

                return JObject.Parse(dataString).ToObject<WebConfiguration>()
                    ?? throw new Exception("Failed to deserialize 'Data' into WebConfiguration.");
            }
            catch (RequestFailedException e)
            {
                throw new Exception("Could not connect to Storage to retrieve settings.", e);
            }

        }
    }
}
