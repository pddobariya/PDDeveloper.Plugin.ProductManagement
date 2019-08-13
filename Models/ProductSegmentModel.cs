using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace PDDeveloper.Plugin.ProductManagement.Models
{
    public class ProductSegmentModel : BaseNopEntityModel
    {
        #region Ctor
        public ProductSegmentModel()
        {
            AvailableStores = new List<SelectListItem>();
            ProductSegmentOptionSearchModel = new ProductSegmentOptionSearchModel();
        }
        #endregion

        #region Properties
        [NopResourceDisplayName("Plugins.PDD.ProductManagement.Segment.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Plugins.PDD.ProductManagement.Segment.Fields.Description")]
        public string Description { get; set; }

        [NopResourceDisplayName("Plugins.PDD.ProductManagement.Segment.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }
        
        public List<SelectListItem> AvailableStores { get; set; }
        [NopResourceDisplayName("Plugins.PDD.ProductManagement.Segment.Fields.StoreId")]
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        
        public ProductSegmentOptionSearchModel ProductSegmentOptionSearchModel { get; set; }
        #endregion
    }
}
