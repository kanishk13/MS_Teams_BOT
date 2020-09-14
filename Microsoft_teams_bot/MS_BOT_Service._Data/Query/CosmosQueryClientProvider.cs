using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MS.GTA.BOTService.Data.Contracts;
using MS.GTA.BOTService.Data.Interfaces;
using System;
using System.Collections.Generic;

namespace MS.GTA.BOTService.Data.Query
{
    /// <summary>
    /// Cosmos Query Client Provider provides the query for a given database and container
    /// </summary>
    public class CosmosQueryClientProvider : ICosmosQueryClientProvider
    {
        private ILogger<CosmosQueryClientProvider> logger;
        private CosmosDBConfiguration cosmosDBConfiguration;
        private IDocumentClient documentClient;

        /// <summary>
        /// The Cosmos Query client dictionary
        /// </summary>
        private static Dictionary<string, ICosmosQueryClient> _cosmostClientDictionary = new Dictionary<string, ICosmosQueryClient>();

        public CosmosQueryClientProvider(IOptions<CosmosDBConfiguration> cosmosDBConfiguration, ILogger<CosmosQueryClientProvider> logger)
        {
            this.logger = logger;
            this.cosmosDBConfiguration = cosmosDBConfiguration?.Value;
            this.cosmosDBConfiguration = new CosmosDBConfiguration
            {
                Key = "",
                Database = "",
                Uri = "",
                GTACommonContainer = "",
            };

            this.documentClient = this.GetDocumentClient();
        }

        /// <summary>Get or generate a Cosmos Query client.</summary>
        /// <param name="containerName">Name of the container in database</param>
        /// <param name="databaseName">Name of the database in CosmosDB account</param>
        /// <returns>Query for the container</returns>
        public ICosmosQueryClient GetCosmosQueryClient(string containerName, string databaseName)
        {
            string key = GetKey(containerName, databaseName);
            if (_cosmostClientDictionary.ContainsKey(key))
            {
                return _cosmostClientDictionary[key];
            }
            else
            { 
                var cosmosQueryClient = new CosmosQueryClient(this.documentClient, databaseName, containerName);
                _cosmostClientDictionary[key] = cosmosQueryClient;
                return cosmosQueryClient;
            }
        }

        private IDocumentClient GetDocumentClient()
        {
            var resourceAccessKey = cosmosDBConfiguration?.Key;
            var resourceEndpoint = cosmosDBConfiguration?.Uri;
            var connectionPolicy = new ConnectionPolicy() { ConnectionMode = ConnectionMode.Direct, ConnectionProtocol = Protocol.Tcp };

            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.logger.LogWarning($"Program has debugger attached and will use https for docdb calls, this will reduce performance but allow for tracing.");

                connectionPolicy.ConnectionProtocol = Protocol.Https;
            }

            return new DocumentClient(new Uri(resourceEndpoint), resourceAccessKey, connectionPolicy: connectionPolicy);
        }

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="databaseName">Name of the database.</param>
        /// <returns>Key</returns>
        private string GetKey(string containerName, string databaseName)
        {
            return containerName + "-" + databaseName;
        }
    }
}