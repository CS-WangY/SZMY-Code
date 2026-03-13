using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace com.mymoo.workbench.cache.ElasticSearch
{
    public interface IElasticSearchService
    {
        //void AddProduct(string index,ProductTestDto product);
        //void DeleteProduct(string index, int productId);
        //List<ProductTestDto> GetProducts(string index, int productId);
        //object QueryDocument<T>(string index);

        Task AddAsync<T>(string indexName, T doc) where T : class;

        Task AddMultAsync<T>(string indexName, List<T> doc) where T : class;

        Task UpdateAsync<T>(string indexName,string field, string[] param) where T : class;

        Task<T> GetDocumentAsync<T>(string indexName, string field, string[] param) where T : class;

        Task<List<T>> GetDocumentsAsync<T>(string indexName, string field, string[] param) where T : class;
        Task DeleteDocumentAsync<T>(string indexName, T document) where T : class;
    }
}
