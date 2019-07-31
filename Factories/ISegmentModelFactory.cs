using PDDeveloper.Plugin.ProductManagement.Domain;
using PDDeveloper.Plugin.ProductManagement.Models;
using Nop.Core.Domain.Catalog;
using Nop.Web.Areas.Admin.Models.Catalog;
using System.Collections.Generic;

namespace PDDeveloper.Plugin.ProductManagement.Factories
{
    public interface ISegmentModelFactory
    {
        /// <summary>
        /// Prepare segment search model
        /// </summary>
        /// <param name="searchModel">segment model</param>
        /// <returns>segmetn model</returns>
        ProductSegmentSearchModel PrepareSegmentSearchModel(ProductSegmentSearchModel searchModel);

        /// <summary>
        /// Prepare paged product segment list model
        /// </summary>
        /// <param name="searchModel">Product segment search model</param>
        /// <returns>Product segment list model</returns>
        ProductSegmentListModel PrepareProductSegmentListModel(ProductSegmentSearchModel searchModel);

        /// <summary>
        /// Prepare product segment model
        /// </summary>
        /// <param name="model">Product segment model</param>
        /// <param name="productSegment">productSegment</param>
        /// <returns>ProductSegmentModel</returns>
        ProductSegmentModel PrepareProductSegmentModel(ProductSegmentModel model, PDD_ProductSegment productSegment);

        /// <summary>
        /// Prepare product attribute mapping model
        /// </summary>
        /// <param name="model">Product attribute mapping model</param>
        /// <param name="product">Product</param>
        /// <param name="productAttributeMapping">Product attribute mapping</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Product attribute mapping model</returns>
        Models.ProductAttributeMappingModel PrepareProductAttributeMappingModel(Models.ProductAttributeMappingModel model,
            int productSegmentId, int productAttributeId,ProductAttributeMapping productAttributeMapping, bool excludeProperties = false);

        /// <summary>
        /// Prepare product attribute value model
        /// </summary>
        /// <param name="model">Product attribute value model</param>
        /// <param name="productAttributeMapping">Product attribute mapping</param>
        /// <param name="productAttributeValue">Product attribute value</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Product attribute value model</returns>
        Models.ProductAttributeValueModel PrepareProductAttributeValueModel(Models.ProductAttributeValueModel model,
            ProductAttributeMapping productAttributeMapping, ProductAttributeValue productAttributeValue, bool excludeProperties = false);

        /// <summary>
        /// Prepare paged product specification attribute list model
        /// </summary>
        /// <param name="searchModel">Product specification attribute search model</param>
        /// <param name="product">Product</param>
        /// <returns>Product specification attribute list model</returns>
        Models.ProductSpecificationAttributeListModel PrepareProductSpecificationAttributeListModel(
            Models.ProductSpecificationAttributeModel searchModel, IList<Product> products);
    }
}
