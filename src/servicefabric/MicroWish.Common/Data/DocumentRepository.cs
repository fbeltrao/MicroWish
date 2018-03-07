using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MicroWish.Data
{
    public class DocumentRepository<TEntity>
    {
        private readonly string collectionName;
        private readonly string databaseId;
        private readonly Uri endpoint;
        private readonly string authKey;        

        public DocumentRepository()
        {
        }

        
        public DocumentRepository(Uri endpoint, string databaseId, string collectionName, string authKey)
        {
            this.endpoint = endpoint;
            this.databaseId = databaseId;
            this.collectionName = collectionName;
            this.authKey = authKey;         
        }


        #region Helpers

        DocumentClient CreateConnection()
        {
            DocumentClient client = new DocumentClient(
                this.endpoint,
                this.authKey
                );

            return client;
        }
        #endregion

        public async Task<TEntity> GetSingle(object key, string partitionKeyValue)
        {
            try
            {
                Uri documentUri = UriFactory.CreateDocumentUri(databaseId, collectionName, key.ToString());
                using (var client = CreateConnection())
                {
                    return await client.ReadDocumentAsync<TEntity>(documentUri, new RequestOptions()
                    {
                        PartitionKey = new PartitionKey(partitionKeyValue)
                    }
                    );
                }
            }
            catch (DocumentClientException dce)
            {
                if (dce.StatusCode == HttpStatusCode.NotFound)
                {
                    return default(TEntity);
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                // TODO: handle error
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<TEntity> GetSingle(object key)
        {
            try
            {
                Uri documentUri = UriFactory.CreateDocumentUri(databaseId, collectionName, key.ToString());
                using (var client = CreateConnection())
                {
                    return await client.ReadDocumentAsync<TEntity>(documentUri);
                }
            }
            catch (DocumentClientException dce)
            {
                if (dce.StatusCode == HttpStatusCode.NotFound)
                {
                    return default(TEntity);
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                // TODO: handle error
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }
        }


        public async Task<TEntity> Create(TEntity value)
        {
            try
            {

                Uri collectionUri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionName);
                using (var client = CreateConnection())
                {
                    var createdDocument = await client.CreateDocumentAsync(collectionUri, value);
                    if (createdDocument.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        using (var reader = new StreamReader(createdDocument.ResponseStream))
                        {
                            return JsonConvert.DeserializeObject<TEntity>(await reader.ReadToEndAsync());
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                // TODO: handle error
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }

            return default(TEntity);
        }

        public async Task<TEntity> Create(TEntity value, string partitionKeyValue)
        {
            try
            {
                Uri collectionUri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionName);
                using (var client = CreateConnection())
                {
                    var createdDocument = await client.CreateDocumentAsync(collectionUri, value, new RequestOptions()
                    {
                        PartitionKey = new PartitionKey(partitionKeyValue)
                    });

                    if (createdDocument.StatusCode == System.Net.HttpStatusCode.Created)
                    {
                        using (var reader = new StreamReader(createdDocument.ResponseStream))
                        {
                            return JsonConvert.DeserializeObject<TEntity>(await reader.ReadToEndAsync());
                        }
                    }                    
                }
                
            }
            catch (Exception ex)
            {
                // TODO: handle error
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }

            return default(TEntity);
        }

        public async Task<TEntity> Update(string id, TEntity value)
        {
            try
            {
                Uri documentUri = UriFactory.CreateDocumentUri(databaseId, collectionName, id);
                using (var client = CreateConnection())
                {
                    var response = await client.ReplaceDocumentAsync(documentUri, value);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        using (var reader = new StreamReader(response.ResponseStream))
                        {
                            return JsonConvert.DeserializeObject<TEntity>(await reader.ReadToEndAsync());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO: handle error
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }

            return default(TEntity);
        }

        public async Task<TEntity> Update(string id, string partitionKeyValue, TEntity value)
        {
            try
            {
                Uri documentUri = UriFactory.CreateDocumentUri(databaseId, collectionName, id);
                using (var client = CreateConnection())
                {
                    var response = await client.ReplaceDocumentAsync(documentUri, value, new RequestOptions()
                    {
                        PartitionKey = new PartitionKey(partitionKeyValue)
                    });

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        using (var reader = new StreamReader(response.ResponseStream))
                        {
                            return JsonConvert.DeserializeObject<TEntity>(await reader.ReadToEndAsync());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO: handle error
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }

            return default(TEntity);
        }

        public async Task<IEnumerable<TEntity>> List()
        {
            var list = new List<TEntity>();


            try
            {
                using (var client = CreateConnection())
                {
                    var query = client.CreateDocumentQuery<TEntity>(UriFactory.CreateDocumentCollectionUri(databaseId, collectionName)).AsDocumentQuery();
                    while (query.HasMoreResults)
                    {
                        list.AddRange(await query.ExecuteNextAsync<TEntity>());
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO: handle error
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }


            return list;
        }
    }
}
