namespace PDDeveloper.Plugin.ProductManagement.Models
{
    public class ProductAttributeMappingModel : Nop.Web.Areas.Admin.Models.Catalog.ProductAttributeMappingModel
    {
        public ProductAttributeMappingModel()
        {
            PDD_ProductAttributeValueSearchModel = new Models.ProductAttributeValueSearchModel();
        }

        public int PDD_ProductAttributeMapId { get; set; }

        public int ProductSegmentId { get; set; }

        public string AttributeMappedIds { get; set; }

        public Models.ProductAttributeValueSearchModel PDD_ProductAttributeValueSearchModel { get; set; }
    }
}
