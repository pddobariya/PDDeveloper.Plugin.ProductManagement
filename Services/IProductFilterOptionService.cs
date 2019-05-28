using GBS.Plugin.ProductManagement.Domain;
using GBS.Plugin.ProductManagement.Domain.Enums;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using System.Collections.Generic;

namespace GBS.Plugin.ProductManagement.Services
{
    public interface IProductFilterOptionService
    {
        /// <summary>
        /// Gets all opction
        /// </summary>
        /// <param name="segmentId">Segment Identity</param>
        /// <returns>List of product segment opction</returns>
        IList<ProductFilterOptions> GetAllFilterOptionBySegmentId(int segmentId = 0);

        /// <summary>
        /// Inserts Product filter options
        /// </summary>
        /// <param name="productFilterOption">ProductFilterOptions</param>
        void InsertProductFilterOption(ProductFilterOptions productFilterOption);

        /// <summary>
        /// Updates the ProductFilterOptions
        /// </summary>
        /// <param name="productFilterOption">ProductFilterOptions</param>
        void UpdateProductFilterOption(ProductFilterOptions productFilterOption);

        /// <summary>
        /// Deletes a productFilterOption
        /// </summary>
        /// <param name="productFilterOption">productFilterOption</param>
        void DeleteProductFilterOption(ProductFilterOptions productFilterOption);

        /// <summary>
        /// Get product segment option by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>productFilterOption</returns>
        ProductFilterOptions GetProductFilterOptionById(int id);

        #region Method for Include & Exclude
        /// <summary>
        /// Get all Filter opction
        /// </summary>
        /// <param name="segmentId">Segment Identity</param>
        /// <returns></returns>
        IList<Product_Include_Exclude> GetAllIncludeExcludeProductBySegmentId(int segmentId, SegmentProductType segmentProductType, int productId = 0);

        /// <summary>
        /// Insert IncludeExcludeProduct
        /// </summary>
        /// <param name="product_Include_Exclude"></param>
        void InsertIncludeExcludeProduct(Product_Include_Exclude product_Include_Exclude);

        /// <summary>
        /// Updte IncludeExcludeProduct
        /// </summary>
        /// <param name="product_Include_Exclude"></param>
        void UpdateIncludeExcludeProduct(Product_Include_Exclude product_Include_Exclude);

        /// <summary>
        /// Delete IncludeExcludeProduct
        /// </summary>
        /// <param name="product_Include_Exclude"></param>
        void DeleteIncludeExcludeProduct(Product_Include_Exclude product_Include_Exclude);

        /// <summary>
        /// get IncludeExcludeProduct
        /// </summary>
        /// <param name="id">Opction identity</param>
        /// <returns>Product_Include_Exclude</returns>
        Product_Include_Exclude GetIncludeExcludeProductById(int id);

        /// <summary>
        /// Get product list by segment id
        /// </summary>
        /// <param name="productSegmentManagerId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        IPagedList<Product> GetProductsBySegmentId(int productSegmentManagerId, int pageIndex = 0, int pageSize = int.MaxValue);
        #endregion
    }
}
