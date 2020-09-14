namespace MS.GTA.BOTService.Data.Interfaces
{
    public interface ICosmosQueryClientProvider
    {
        /// <summary>Get or generate a Cosmos Query client.</summary>
        /// <param name="containerName">Name of the container in database</param>
        /// <param name="databaseName">Name of the database in CosmosDB account</param>
        /// <returns>Query for the container</returns>
        ICosmosQueryClient GetCosmosQueryClient(string databaseName, string containerName);
    }
}