using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models;

namespace GBS.Plugin.ProductManagement.Models
{
    public class ProductSpecificationAttributeModel : Nop.Web.Areas.Admin.Models.Catalog.ProductSpecificationAttributeModel
    {
        public ProductSpecificationAttributeModel()
        {
            AddSpecificationAttributeModel = new AddSpecificationAttributeToProductModel();
            ProductSpecificationAttributeSearchModel = new ProductSpecificationAttributeSearchModel();
        }

        //add specification attribute model
        public AddSpecificationAttributeToProductModel AddSpecificationAttributeModel { get; set; }

        public ProductSpecificationAttributeSearchModel ProductSpecificationAttributeSearchModel { get; set; }
        
        public int ProductSegmentId { get; set; }

        public int ProductSpecificationId { get; set; }

        public int GBS_ProductAttributeMapId { get; set; }
    }
}
