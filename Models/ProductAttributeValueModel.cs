namespace PDDeveloper.Plugin.ProductManagement.Models
{
    public class ProductAttributeValueModel : Nop.Web.Areas.Admin.Models.Catalog.ProductAttributeValueModel
    {
        public int PDD_ProductAttributeMapId { get; set; }

        public int ProductAttributeId { get; set; }

        public int ProductSegmentId { get; set; }

        public string AttributeMappedIds { get; set; }
    }
}
