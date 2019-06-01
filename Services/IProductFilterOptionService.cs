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
        IList<GBS_ProductFilterOptions> GetAllFilterOptionBySegmentId(int segmentId = 0);

        /// <summary>
        /// Inserts Product filter options
        /// </summary>
        /// <param name="productFilterOption">ProductFilterOptions</param>
        void InsertProductFilterOption(GBS_ProductFilterOptions productFilterOption);

        /// <summary>
        /// Updates the ProductFilterOptions
        /// </summary>
        /// <param name="productFilterOption">ProductFilterOptions</param>
        void UpdateProductFilterOption(GBS_ProductFilterOptions productFilterOption);

        /// <summary>
        /// Deletes a productFilterOption
        /// </summary>
        /// <param name="productFilterOption">productFilterOption</param>
        void DeleteProductFilterOption(GBS_ProductFilterOptions productFilterOption);

        /// <summary>
        /// Get product segment option by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>productFilterOption</returns>
        GBS_ProductFilterOptions GetProductFilterOptionById(int id);

        #region Method for Include & Exclude
        /// <summary>
        /// Get all Filter opction
        /// </summary>
        /// <param name="segmentId">Segment Identity</param>
        /// <returns></returns>
        IList<GBS_Product_Include_Exclude> GetAllIncludeExcludeProductBySegmentId(int segmentId, SegmentProductType segmentProductType, int productId = 0);

        /// <summary>
        /// Insert IncludeExcludeProduct
        /// </summary>
        /// <param name="product_Include_Exclude"></param>
        void InsertIncludeExcludeProduct(GBS_Product_Include_Exclude product_Include_Exclude);

        /// <summary>
        /// Updte IncludeExcludeProduct
        /// </summary>
        /// <param name="product_Include_Exclude"></param>
        void UpdateIncludeExcludeProduct(GBS_Product_Include_Exclude product_Include_Exclude);

        /// <summary>
        /// Delete IncludeExcludeProduct
        /// </summary>
        /// <param name="product_Include_Exclude"></param>
        void DeleteIncludeExcludeProduct(GBS_Product_Include_Exclude product_Include_Exclude);

        /// <summary>
        /// get IncludeExcludeProduct
        /// </summary>
        /// <param name="id">Opction identity</param>
        /// <returns>Product_Include_Exclude</returns>
        GBS_Product_Include_Exclude GetIncludeExcludeProductById(int id);

        /// <summary>
        /// Get product list by segment id
        /// </summary>
        /// <param name="productSegmentManagerId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        IList<Product> GetProductsBySegmentId(int productSegmentManagerId, out int totalRecords, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get product attribute list by segment id
        /// </summary>
        /// <param name="productSegmentManagerId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        IList<ProductAttribute> GetProductAttributeBySegmentId(int productSegmentManagerId, out int totalRecords, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get specification attribute list by segment id
        /// </summary>
        /// <param name="productSegmentManagerId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        IList<SpecificationAttribute> GetProductSpecificationAttributeBySegmentId(int productSegmentManagerId, out int totalRecords, int pageIndex = 0, int pageSize = int.MaxValue);
        #endregion
    }
}
