using GBS.Plugin.ProductManagement.Domain;
using GBS.Plugin.ProductManagement.Models;

namespace GBS.Plugin.ProductManagement.Factories
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
        ProductSegmentModel PrepareProductSegmentModel(ProductSegmentModel model, ProductSegment productSegment);
    }
}
