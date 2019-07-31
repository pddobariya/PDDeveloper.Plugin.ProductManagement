using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace PDDeveloper.Plugin.ProductManagement.Models
{
    //[Validator(typeof(ProductSegmentValidator))]
    public class ProductFilterOptionsModel : BaseNopEntityModel
    {
        #region Ctor
        public ProductFilterOptionsModel()
        {
        }
        #endregion

        #region Properties
        [NopResourceDisplayName("Plugins.GBS.ProductManagement.SegmentOption.Fields.ProductSegmentManagerId")]
        public int ProductSegmentManagerId { get; set; }

        [NopResourceDisplayName("Plugins.GBS.ProductManagement.SegmentOption.Fields.BeginsWith")]
        public string BeginsWith { get; set; }

        [NopResourceDisplayName("Plugins.GBS.ProductManagement.SegmentOption.Fields.EndsWith")]
        public string EndsWith { get; set; }

        [NopResourceDisplayName("Plugins.GBS.ProductManagement.SegmentOption.Fields.DoesNotEndWith")]
        public string DoesNotEndWith { get; set; }

        [NopResourceDisplayName("Plugins.GBS.ProductManagement.SegmentOption.Fields.Contains")]
        public string Contains { get; set; }
        #endregion
    }
}
