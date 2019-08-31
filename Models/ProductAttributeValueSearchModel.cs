namespace PDDeveloper.Plugin.ProductManagement.Models
{
    public class ProductAttributeValueSearchModel : Nop.Web.Areas.Admin.Models.Catalog.ProductAttributeValueSearchModel
    {
        public string AttributeMappedIds { get; set; }

        public int ProductSegmentId { get; set; }

        public int ProductAttributeId { get; set; }
    }
}
