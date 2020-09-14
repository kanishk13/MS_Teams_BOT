using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Logging;
using MS.GTA.BOTService.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MS.GTA.BOTService.Data.Query
{
    public class CosmosQueryClient : ICosmosQueryClient
    {
        /// <summary>Gets the collection id.</summary>
        public string CollectionId { get; }

        /// <summary>Gets the database id.</summary>
        public string DatabaseId { get; private set; }

        /// <summary>Gets the document client.</summary>
        public IDocumentClient DocumentClient { get; }

        /// <summary>The request options.</summary>
        private readonly RequestOptions requestOptions;

        private string databaseName;
        private string containerName;


        /// <summary>Initializes a new instance of the <see cref="CosmosQueryClient"/> class.</summary>
        /// <param name="documentClient">The document client.</param>
        /// <param name="databaseId">The database id.</param>
        /// <param name="collectionId">The collection id.</param>
        /// <param name="partitionKey">The partition Key.</param>
        /// <param name="triggerName">The trigger Name.</param>
        public CosmosQueryClient(IDocumentClient documentClient, string databaseId, string collectionId, string partitionKey = null)
        {
            this.CollectionId = collectionId;
            this.DatabaseId = databaseId;
            this.DocumentClient = documentClient;
        }

        /// <summary>The get document uri.</summary>
        /// <param name="documentId">The document id.</param>
        /// <returns>The <see cref="Uri"/>.</returns>
        public Uri GetDocumentUri(string documentId)
        {
            return UriFactory.CreateDocumentUri(this.DatabaseId, this.CollectionId, documentId);
        }

        /// <summary>The get collection uri.</summary>
        /// <returns>The <see cref="Uri"/>.</returns>
        public Uri GetCollectionUri()
        {
            return UriFactory.CreateDocumentCollectionUri(this.DatabaseId, this.CollectionId);
        }

        /// <summary>The get first or default.</summary>
        /// <param name="expression">The expression.</param>
        /// <param name="feedOptions">The feed options.</param>
        /// <typeparam name="T">The generic type T</typeparam>
        /// <returns>The <see cref="Task"/>.</returns>
        public async Task<T> GetFirstOrDefault<T>(Expression<Func<T, bool>> expression, FeedOptions feedOptions = null)
            where T : class
        {
            var result = await this.Get(expression, feedOptions);
            return result?.FirstOrDefault();
        }

        /// <summary>
        /// Gets the with pagination.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">The expression.</param>
        /// <param name="feedOptions">The feed options.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="take">The take.</param>
        /// <returns>
        /// The <see cref="Task" />.
        /// </returns>
        public async Task<IEnumerable<T>> GetWithPagination<T>(Expression<Func<T, bool>> expression, FeedOptions feedOptions = null, int skip = 0, int take = 0)
          where T : class
        {
            IDocumentQuery<T> query;
            if (skip == 0 && take == 0)
            {
                query = this.DocumentClient.CreateDocumentQuery<T>(this.GetCollectionUri(), feedOptions ?? this.GetFeedOptions<T>())
                    .Where(expression)
                    .AsDocumentQuery();
            }
            else
            {
                query = this.DocumentClient.CreateDocumentQuery<T>(this.GetCollectionUri(), feedOptions ?? this.GetFeedOptions<T>())
                    .Where(expression)
                    .Skip(skip)
                    .Take(take)
                    .AsDocumentQuery();
            }
            return await this.ReadPrivate<T, T>(query);
        }

        /// <summary>The get.</summary>
        /// <param name="expression">The expression.</param>
        /// <param name="feedOptions">The feed options.</param>
        /// <typeparam name="T">The generic type T</typeparam>
        /// <returns>The <see cref="Task"/>.</returns>
        public async Task<IEnumerable<T>> Get<T>(Expression<Func<T, bool>> expression, FeedOptions feedOptions = null)
            where T : class
        {
            //logger.LogInformation($"Getting {typeof(T).Name} with passed in expression");

            var query = this.DocumentClient.CreateDocumentQuery<T>(this.GetCollectionUri(), feedOptions ?? this.GetFeedOptions<T>())
                    .Where(expression)
                    .AsDocumentQuery();
            return await this.ReadPrivate<T, T>(query);

        }

        /// <summary>The get feed options.</summary>
        /// <typeparam name="T">The generic type</typeparam>
        /// <returns>The <see cref="FeedOptions"/>.</returns>
        private FeedOptions GetFeedOptions<T>()
            where T : class
        {
            var feedOptions = new FeedOptions { EnableCrossPartitionQuery = true, MaxDegreeOfParallelism = -1 };

            //if (typeof(T).IsSubclassOf(typeof(DocDbEntity)))
            //{
            //    feedOptions.PartitionKey = new PartitionKey(typeof(T).Name);
            //}

            return feedOptions;
        }

        /// <summary>
        /// The get request options.
        /// </summary>
        /// <typeparam name="T">The generic type.</typeparam>
        /// <returns>The <see cref="RequestOptions"/>.</returns>
        private RequestOptions GetRequestOptions<T>() where T : class
        { 
        //{
        //    if (typeof(T).IsSubclassOf(typeof(DocDbEntity)))
        //    {
        //        this.requestOptions.PartitionKey = new PartitionKey(typeof(T).Name);
        //    }

            return this.requestOptions;
        }

        /// <summary>The read private.</summary>
        /// <param name="documentQuery">The document query.</param>
        /// <param name="retry">The retry.</param>
        /// <typeparam name="T">The type of document to read.</typeparam>
        /// <typeparam name="BackfillCheckType">
        /// The backfill check type. In scenarios where the return type is actually diffrent than the read type.
        /// i.e you use OnboardingGuide as your querytype but when passed to Read the return type is string this can mess with the
        /// type we check for the backfill attribute and cause it to backfill unexpectedly since we default to doing the backfill.
        /// </typeparam>
        /// <returns>The <see cref="Task"/>.</returns>
        private async Task<IList<T>> ReadPrivate<T, BackfillCheckType>(IDocumentQuery<T> documentQuery, bool retry = false)
            where T : class
            where BackfillCheckType : class
        {
            if(documentQuery == null)
            {
                throw new InvalidDataException($"Query expression cannot be null");
            }


            try
            {
                var results = new List<T>();

                while (documentQuery.HasMoreResults)
                {
                    var response = await documentQuery.ExecuteNextAsync<T>();
                    this.ProcessFeedResponse(response);
                    if (response != null && response.Any())
                    {
                        //this.logger.LogInformation($"Fetching results for {typeof(T)}");
                        results.AddRange(response);
                    }
                }

                return results;
            }
            catch (Exception e)
            {
                //this.logger.LogError($"Exception in Reading from Database: {e.Message}");
                throw;
            }
        }

        /// <summary>The process feed response.</summary>
        /// <param name="feedResponse">The feed response.</param>
        /// <typeparam name="T">The generic type.</typeparam>
        private void ProcessFeedResponse<T>(FeedResponse<T> feedResponse)
        {
            try
            {
                //this.logger.LogInformation($"Read a page of {feedResponse?.Count} elements, ActivityId: {feedResponse?.ActivityId}, Charge: {feedResponse?.RequestCharge}");
            }
            catch (NullReferenceException e)
            {
                //// This should only occur when mocking
                //this.logger.LogWarning($"Unable to log feed response information {e}");
            }
        }
    }
}
