using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using PDDeveloper.Plugin.ProductManagement.Domain.Enums;
using System.Collections.Generic;

namespace PDDeveloper.Plugin.ProductManagement.Models
{
    public class IncludeExcludeProductModel  : BaseNopEntityModel
    {
        public int ProductSegmentId { get; set; }

        public int ProductId { get; set; }

        [NopResourceDisplayName("Plugins.PDD.ProductManagement.IncludeProduct.Fields.Name")]
        public string ProductName { get; set; }

        [NopResourceDisplayName("Plugins.PDD.ProductManagement.IncludeProduct.Fields.ProductType")]
        public string ProductType { get; set; }
    }

    public partial class IncludeExcludeProductSearchModel : BaseSearchModel
    {
        public IncludeExcludeProductSearchModel()
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

        public int ProductSegmentId { get; set; }

        public SegmentProductType ProductType { get; set; }

        public int WidgetId { get; set; }

        public int[] SelectedProductIds { get; set; }

    }

    public class SegmentProducts : BaseNopModel
    {
        public int Id { get; set; }
        [NopResourceDisplayName("Plugins.PDD.ProductManagement.Products.Fields.Name")]
        public string Name { get; set; }
        [NopResourceDisplayName("Plugins.PDD.ProductManagement.Products.Fields.Sku")]
        public string Sku { get; set; }
    }

    public class ProductAttributes : BaseNopModel
    {
        public int Id { get; set; }
        [NopResourceDisplayName("Plugins.PDD.ProductManagement.ProductAttributes.Fields.Name")]
        public string Name { get; set; }

        public int ProductId { get; set; }
        public bool isAttributeAdded { get; set; }
    }

    public class ProductSpecificationAttributes : BaseNopModel
    {
        public int Id { get; set; }
        [NopResourceDisplayName("Plugins.PDD.ProductManagement.ProductSpecificationAttributes.Fields.Name")]
        public string Name { get; set; }
    }
}
