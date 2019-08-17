using Nop.Web.Framework.Models;

namespace PDDeveloper.Plugin.ProductManagement.Models
{
    public partial class ProductAttributesListModel : BasePagedListModel<ProductAttributes>
    {
    }

    public partial class SegmentProductAttributeSearchModel : BaseSearchModel
    {
        public int ProductSegmentId { get; set; }
    }
}
