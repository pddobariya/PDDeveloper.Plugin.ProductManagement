using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace GBS.Plugin.ProductManagement.Models
{
    public class IncludeExcludeProductModel  : BaseNopEntityModel
    {
        public int ProductSegmentId { get; set; }

        public int ProductId { get; set; }

        [NopResourceDisplayName("Plugins.GBS.ProductManagement.IncludeProduct.Fields.Name")]
        public string ProductName { get; set; }

        [NopResourceDisplayName("Plugins.GBS.ProductManagement.IncludeProduct.Fields.ProductType")]
        public string ProductType { get; set; }
    }

    public partial class AddIncludeExcludeProductModel : BaseNopModel
    {
        public AddIncludeExcludeProductModel()
        {
            AvailableCategories = new List<SelectListItem>();
            AvailableManufacturers = new List<SelectListItem>();
            AvailableStores = new List<SelectListItem>();
            AvailableVendors = new List<SelectListItem>();
            AvailableProductTypes = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Catalog.Products.List.SearchProductName")]
        public string SearchProductName { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Products.List.SearchCategory")]
        public int SearchCategoryId { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Products.List.SearchManufacturer")]
        public int SearchManufacturerId { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Products.List.SearchStore")]
        public int SearchStoreId { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Products.List.SearchVendor")]
        public int SearchVendorId { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Products.List.SearchProductType")]
        public int SearchProductTypeId { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; }
        public IList<SelectListItem> AvailableManufacturers { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
        public IList<SelectListItem> AvailableVendors { get; set; }
        public IList<SelectListItem> AvailableProductTypes { get; set; }

        public int WidgetId { get; set; }

        public int[] SelectedProductIds { get; set; }
    }
}
