using Nop.Web.Framework.Models;

namespace PDDeveloper.Plugin.ProductManagement.Models
{
    public class ProductSegmentOptionSearchModel : BaseSearchModel
    {
        #region Ctor
        public ProductSegmentOptionSearchModel()
        {
            ProductFilterOptionsModel = new ProductFilterOptionsModel();
        }
        #endregion

        #region Properties
        public int ProductSegmentId { get; set; }

        public ProductFilterOptionsModel ProductFilterOptionsModel { get; set; }
        #endregion
    }
}
