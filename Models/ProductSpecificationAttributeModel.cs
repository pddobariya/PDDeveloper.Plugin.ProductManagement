using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace PDDeveloper.Plugin.ProductManagement.Models
{
    public class ProductSpecificationAttributeModel :  Nop.Web.Areas.Admin.Models.Catalog.ProductSpecificationAttributeModel
    {
        public ProductSpecificationAttributeModel()
        {
            AddSpecificationAttributeModel = new AddSpecificationAttributeModel();
            ProductSpecificationAttributeSearchModel = new ProductSpecificationAttributeSearchModel();
        }

        //add specification attribute model
        public AddSpecificationAttributeModel AddSpecificationAttributeModel { get; set; }

        public ProductSpecificationAttributeSearchModel ProductSpecificationAttributeSearchModel { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.SpecificationAttributes.Fields.SpecificationAttribute")]
        public int SpecificationAttributeId { get; set; }
        public string SpecificationAttributeName { get; set; }

        public int ProductSegmentId { get; set; }

        //public int ProductSpecificationId { get; set; }

        public int PDD_ProductAttributeMapId { get; set; }

    }
}
