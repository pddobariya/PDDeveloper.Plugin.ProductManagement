using FluentValidation.Attributes;
using GBS.Plugin.ProductManagement.Validators;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace GBS.Plugin.ProductManagement.Models
{
    [Validator(typeof(ProductSegmentValidator))]
    public class ProductSegmentModel : BaseNopEntityModel
    {
        #region Ctor
        public ProductSegmentModel()
        {
            this.AvailableStores = new List<SelectListItem>();
        }
        #endregion

        #region Properties
        [NopResourceDisplayName("Plugins.GBS.ProductManagement.Segment.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Plugins.GBS.ProductManagement.Segment.Fields.Description")]
        public string Description { get; set; }

        [NopResourceDisplayName("Plugins.GBS.ProductManagement.Segment.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }
        
        public List<SelectListItem> AvailableStores { get; set; }
        [NopResourceDisplayName("Plugins.GBS.ProductManagement.Segment.Fields.StoreId")]
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        
        #endregion
    }
}
