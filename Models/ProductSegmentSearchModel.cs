using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace GBS.Plugin.ProductManagement.Models
{
    public class ProductSegmentSearchModel : BaseSearchModel
    {
        #region Ctor

        public ProductSegmentSearchModel()
        {
            AvailableStores = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Plugins.GBS.ProductManagement.SegmentSearch.Name")]
        public string SearchSegmentName { get; set; }

        [NopResourceDisplayName("Plugins.GBS.ProductManagement.SegmentSearch.Store")]
        public int SearchStoreId { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }

        #endregion
    }
}
