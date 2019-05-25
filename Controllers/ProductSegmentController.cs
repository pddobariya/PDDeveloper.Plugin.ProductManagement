using GBS.Plugin.ProductManagement.Domain;
using GBS.Plugin.ProductManagement.Domain.Enums;
using GBS.Plugin.ProductManagement.Factories;
using GBS.Plugin.ProductManagement.Models;
using GBS.Plugin.ProductManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Helpers;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GBS.Plugin.ProductManagement.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class ProductSegmentController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly ISegmentModelFactory _segmentModelFactory;
        private readonly IProductSegmentService _productSegmentService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IProductFilterOptionService _productFilterOptionService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IManufacturerService _manufacturerService;
        private readonly IStoreService _storeService;
        private readonly IVendorService _vendorService;
        #endregion

        #region Ctor
        public ProductSegmentController(IPermissionService permissionService,
            ISegmentModelFactory segmentModelFactory,
            IProductSegmentService productSegmentService,
            ILocalizationService localizationService,
            ICustomerActivityService customerActivityService,
            IProductFilterOptionService productFilterOptionService,
            IProductService productService,
            ICategoryService categoryService,
            IStaticCacheManager cacheManager,
            IManufacturerService manufacturerService,
            IStoreService storeService,
            IVendorService vendorService)
        {
            this._permissionService = permissionService;
            this._segmentModelFactory = segmentModelFactory;
            this._productSegmentService = productSegmentService;
            this._localizationService = localizationService;
            this._customerActivityService = customerActivityService;
            this._productFilterOptionService = productFilterOptionService;
            this._productService = productService;
            this._categoryService = categoryService;
            this._cacheManager = cacheManager;
            this._manufacturerService = manufacturerService;
            this._storeService = storeService;
            this._vendorService = vendorService;
        }
        #endregion

        
        #region Methods

        #region Product Segment

        #region List
        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //prepare model
            var model = _segmentModelFactory.PrepareSegmentSearchModel(new ProductSegmentSearchModel());

            return View("~/Plugins/GBS.Plugin.ProductManagement/Views/ProductSegment/List.cshtml", model);
        }

        [HttpPost]
        public virtual IActionResult List(ProductSegmentSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedKendoGridJson();

            //prepare model
            var model = _segmentModelFactory.PrepareProductSegmentListModel(searchModel);

            return Json(model);
        }

        #endregion

        #region Create / Edit / Delete
        public virtual IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //prepare model
            var model = _segmentModelFactory.PrepareProductSegmentModel(new ProductSegmentModel(), null);

            return View("~/Plugins/GBS.Plugin.ProductManagement/Views/ProductSegment/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Create(ProductSegmentModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var productSegment = new ProductSegment
                {
                    Name = model.Name,
                    Description = model.Description,
                    DisplayOrder = model.DisplayOrder,
                    StoreId = model.StoreId,
                    CreatedOnUtc = DateTime.UtcNow
                };

                _productSegmentService.InsertProductSegment(productSegment);

                //activity log
                _customerActivityService.InsertActivity("ProductSegmentAdded",
                    string.Format(_localizationService.GetResource("Plugins.GBS.ProductManagement.Segment.ActivityLog.CreateSegment"), productSegment.Name), productSegment);

                SuccessNotification(_localizationService.GetResource("Plugins.GBS.ProductManagement.Segment.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                //selected tab
                SaveSelectedTabName();

                return RedirectToAction("Edit", new { id = productSegment.Id });
            }

            //prepare model
            model = _segmentModelFactory.PrepareProductSegmentModel(model, null);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //try to get a produt segment with the specified id
            var productSegment = _productSegmentService.GetProductSegmentById(id);
            if (productSegment == null)
                return RedirectToAction("List");

            //prepare model
            var model = _segmentModelFactory.PrepareProductSegmentModel(null, productSegment);

            return View("~/Plugins/GBS.Plugin.ProductManagement/Views/ProductSegment/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Edit(ProductSegmentModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //try to get a category with the specified id
            var productSegment = _productSegmentService.GetProductSegmentById(model.Id);
            if (productSegment == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                productSegment.Name = model.Name;
                productSegment.Description = model.Description;
                productSegment.DisplayOrder = model.DisplayOrder;
                productSegment.StoreId = model.StoreId;
                productSegment.UpdatedOnUtc = DateTime.UtcNow;

                _productSegmentService.UpdateProductSegment(productSegment);

                //activity log
                _customerActivityService.InsertActivity("ProductSegmentEdit",
                    string.Format(_localizationService.GetResource("Plugins.GBS.ProductManagement.Segment.ActivityLog.EditSegment"), productSegment.Name), productSegment);

                SuccessNotification(_localizationService.GetResource("Plugins.GBS.ProductManagement.Segment.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                //selected tab
                SaveSelectedTabName();

                return RedirectToAction("Edit", new { id = productSegment.Id });
            }

            //prepare model
            model = _segmentModelFactory.PrepareProductSegmentModel(model, productSegment);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //try to get a category with the specified id
            var segment = _productSegmentService.GetProductSegmentById(id);
            if (segment == null)
                return RedirectToAction("List");

            _productSegmentService.DeleteProductSegment(segment);

            //activity log
            _customerActivityService.InsertActivity("ProductSegmentDelete",
                string.Format(_localizationService.GetResource("Plugins.GBS.ProductManagement.Segment.ActivityLog.DeletedSegment"), segment.Name), segment);

            return new NullJsonResult();
        }
        #endregion

        #endregion

        #region ProductSegment Opction
        [HttpPost]
        public virtual IActionResult ProductSegmentOpctionList(int productSegmentId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedKendoGridJson();

            //prepare model
            var segmentOpctions = _productFilterOptionService.GetAllFilterOptionBySegmentId(productSegmentId);

            var gridModel = new DataSourceResult
            {
                Data = segmentOpctions.Select(segmentOpction =>
                {
                    return new ProductFilterOptionsModel
                    {
                        Id = segmentOpction.Id,
                        ProductSegmentManagerId = segmentOpction.ProductSegmentManagerId,
                        BeginsWith = segmentOpction.BeginsWith,
                        EndsWith = segmentOpction.EndsWith,
                        DoesNotEndWith = segmentOpction.DoesNotEndWith,
                        Contains = segmentOpction.Contains
                    };
                }),
                Total = segmentOpctions.Count
            };
            return Json(gridModel);
        }

        [HttpPost]
        public virtual IActionResult ProductFilterOptionUpdate(ProductFilterOptionsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var segmentOpction = _productFilterOptionService.GetProductFilterOptionById(model.Id);

            segmentOpction.BeginsWith = model.BeginsWith;
            segmentOpction.EndsWith = model.EndsWith;
            segmentOpction.DoesNotEndWith = model.DoesNotEndWith;
            segmentOpction.Contains = model.Contains;
            segmentOpction.UpdatedOnUtc = DateTime.UtcNow;

            _productFilterOptionService.UpdateProductFilterOption(segmentOpction);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual IActionResult ProductFilterOptionAdd(int productSegmentId, ProductFilterOptionsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var segmentOpction = new ProductFilterOptions
            {
                ProductSegmentManagerId = productSegmentId,
                BeginsWith = model.BeginsWith,
                EndsWith = model.EndsWith,
                DoesNotEndWith = model.DoesNotEndWith,
                Contains = model.Contains,
                CreatedOnUtc = DateTime.UtcNow
            };

            _productFilterOptionService.InsertProductFilterOption(segmentOpction);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual IActionResult ProductFilterOptionDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //try to get a product segment opction with the specified id
            var segmentOpction = _productFilterOptionService.GetProductFilterOptionById(id)
                ?? throw new ArgumentException("No product segment opction found with the specified id", nameof(id));

            _productFilterOptionService.DeleteProductFilterOption(segmentOpction);

            return new NullJsonResult();
        }

        #endregion

        #region IncludeExclude-products

        [HttpPost]
        public virtual IActionResult IncludeExcludeProductList(DataSourceRequest command, int productSegmentId,int productType)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            var includeproducts = _productFilterOptionService.GetAllIncludeExcludeProductBySegmentId(productSegmentId, (SegmentProductType)productType);

            if (includeproducts == null)
                return Content("productSegment not found");


            var gridModel = new DataSourceResult
            {
                Data = includeproducts.Select(product => new IncludeExcludeProductModel
                {
                    Id = product.Id,
                    ProductSegmentId = productSegmentId,
                    ProductId = product.ProductId,
                    ProductType = ((SegmentProductType)product.ProductType).ToString(),
                    ProductName = _productService.GetProductById(product.ProductId).Name,
                }),
                Total = includeproducts.Count
            };

            return Json(gridModel);
        }

        public virtual IActionResult ProductDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            var incExc = _productFilterOptionService.GetIncludeExcludeProductById(id);
            if (incExc == null)
                throw new ArgumentException("No widget content mapping found with the specified id");

            _productFilterOptionService.DeleteIncludeExcludeProduct(incExc);

            return new NullJsonResult();
        }

        public virtual IActionResult ProductAddPopup(int productSegmentId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            var model = new AddIncludeExcludeProductModel();

            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var categories = SelectListHelper.GetCategoryList(_categoryService, _cacheManager, true);
            foreach (var c in categories)
                model.AvailableCategories.Add(c);

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var manufacturers = SelectListHelper.GetManufacturerList(_manufacturerService, _cacheManager, true);
            foreach (var m in manufacturers)
                model.AvailableManufacturers.Add(m);

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var vendors = SelectListHelper.GetVendorList(_vendorService, _cacheManager, true);
            foreach (var v in vendors)
                model.AvailableVendors.Add(v);

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            return View("~/Plugins/GBS.Plugin.ProductManagement/Views/ProductSegment/ProductAddPopup.cshtml", model);
        }

        [HttpPost]
        public virtual IActionResult ProductAddPopupList(DataSourceRequest command, AddIncludeExcludeProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            var gridModel = new DataSourceResult();
            var products = _productService.SearchProducts(
                categoryIds: new List<int> { model.SearchCategoryId },
                manufacturerId: model.SearchManufacturerId,
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                showHidden: true
            );

            gridModel.Data = products.Select(product => product.ToModel<ProductModel>());
            gridModel.Total = products.TotalCount;

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual IActionResult ProductAddPopup(int productSegmentId, string btnId, string formId,int productType, AddIncludeExcludeProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            if (model.SelectedProductIds != null)
            {
                foreach (int id in model.SelectedProductIds)
                {
                    var product = _productService.GetProductById(id);
                    if (product != null)
                    {
                        int existingIncludeProductMapping = _productFilterOptionService.GetAllIncludeExcludeProductBySegmentId(productSegmentId, (SegmentProductType)productType, id).Count();
                        if (existingIncludeProductMapping == 0)
                        {
                            _productFilterOptionService.InsertIncludeExcludeProduct(
                                new Product_Include_Exclude
                                {
                                    ProductSegmentManagerId = productSegmentId,
                                    ProductType = productType,
                                    ProductId = id,
                                });
                        }
                    }
                }
            }

            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;
            return View("~/Plugins/GBS.Plugin.ProductManagement/Views/ProductSegment/ProductAddPopup.cshtml", model);
        }

        #endregion

        #endregion
    }
}

