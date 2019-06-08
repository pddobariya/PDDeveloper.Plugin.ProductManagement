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
using Nop.Web.Areas.Admin.Factories;
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
        private readonly IProductAttributeService _productAttributeService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IProductModelFactory _productModelFactory;
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
            IVendorService vendorService,
            IProductAttributeService productAttributeService,
            ILanguageService languageService,
            ILocalizedEntityService localizedEntityService,
            IProductModelFactory productModelFactory)
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
            this._productAttributeService = productAttributeService;
            this._languageService = languageService;
            this._localizedEntityService = localizedEntityService;
            this._productModelFactory = productModelFactory;
        }
        #endregion

        #region Utilities
        protected virtual void UpdateLocales(ProductAttributeMapping pam, Models.ProductAttributeMappingModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(pam,
                    x => x.TextPrompt,
                    localized.TextPrompt,
                    localized.LanguageId);
            }
        }

        protected virtual void UpdateLocales(ProductAttributeValue pav, Models.ProductAttributeValueModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(pav,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);
            }
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
                var productSegment = new GBS_ProductSegment
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

            var segmentOpction = new GBS_ProductFilterOptions
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
        public virtual IActionResult IncludeExcludeProductList(DataSourceRequest command, int productSegmentId, int productType)
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
        public virtual IActionResult ProductAddPopup(int productSegmentId, string btnId, string formId, int productType, AddIncludeExcludeProductModel model)
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
                                new GBS_Product_Include_Exclude
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

        #region Products

        [HttpPost]
        public virtual IActionResult ProductList(DataSourceRequest command, int productSegmentId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            int totalRecords = 0;
            var products = _productFilterOptionService.GetProductsBySegmentId(productSegmentId, out totalRecords, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = products.Select(product => new Products
                {
                    Id = product.Id,
                    Name = product.Name,
                    Sku = product.Sku
                }),
                Total = totalRecords
            };

            return Json(gridModel);
        }

        public virtual IActionResult ProductExclude(int productSegmentId, int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            _productFilterOptionService.InsertIncludeExcludeProduct(
                    new GBS_Product_Include_Exclude
                    {
                        ProductSegmentManagerId = productSegmentId,
                        ProductType = (int)SegmentProductType.Exclude,
                        ProductId = id,
                    });

            return new NullJsonResult();
        }

        #endregion

        #region Products attribute
        [HttpPost]
        public virtual IActionResult ProductAttributeList(DataSourceRequest command, int productSegmentId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            int totalRecords = 0;
            var productAttributes = _productFilterOptionService.GetProductAttributeBySegmentId(productSegmentId, out totalRecords, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = productAttributes.Select(productAttribute => new ProductAttributes
                {
                    Id = productAttribute.Id,
                    Name = productAttribute.Name,
                    isAttributeAdded = _productFilterOptionService.GetProductAttributeMapWithSegment(productAttribute.Id, EntityTypeEnum.ProductAttributeMap, productSegmentId).Count() > 0
                }),
                Total = totalRecords
            };

            return Json(gridModel);
        }

        public IActionResult ProductAttributeMappingCreate(int productSegmentId, int productAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            //prepare model
            var model = _segmentModelFactory.PrepareProductAttributeMappingModel(new Models.ProductAttributeMappingModel(), productSegmentId, productAttributeId, null);

            if (model.ProductAttributeId == 0)
                return RedirectToAction("Edit", new { id = model.ProductSegmentId });

            return View("~/Plugins/GBS.Plugin.ProductManagement/Views/ProductAttribute/ProductAttributeMappingCreate.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult ProductAttributeMappingCreate(Models.ProductAttributeMappingModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            var productAttributeSegmentMapping = _productFilterOptionService.GetProductAttributeMapWithSegment(model.ProductAttributeId, EntityTypeEnum.ProductAttributeMap, model.ProductSegmentId);
            if (productAttributeSegmentMapping != null && productAttributeSegmentMapping.Count > 0)
            {
                //redisplay form
                ErrorNotification(_localizationService.GetResource("Admin.Catalog.Products.ProductAttributes.Attributes.AlreadyExists"));
                
                model = _segmentModelFactory.PrepareProductAttributeMappingModel(model, model.ProductAttributeId, model.ProductSegmentId, null, true);

                return View("~/Plugins/GBS.Plugin.ProductManagement/Views/ProductAttribute/ProductAttributeMappingCreate.cshtml", model);
            }

            int totalRecords = 0;
            var products = _productFilterOptionService.GetProductsBySegmentId(model.ProductSegmentId, out totalRecords);

            string attributeMappingId = "";
            for (int i = 0; i < products.Count; i++)
            {
                //ensure this attribute is not mapped yet
                var productAttributeMap = _productAttributeService.GetProductAttributeMappingsByProductId(products[i].Id)
                    .Where(x => x.ProductAttributeId == model.ProductAttributeId).ToList();
                if (productAttributeMap.Count == 0)
                {
                    //insert mapping
                    var productAttributeMapping = new ProductAttributeMapping
                    {
                        ProductId = products[i].Id,
                        ProductAttributeId = model.ProductAttributeId,
                        TextPrompt = model.TextPrompt,
                        IsRequired = model.IsRequired,
                        AttributeControlTypeId = model.AttributeControlTypeId,
                        DisplayOrder = model.DisplayOrder,
                        ValidationMinLength = model.ValidationMinLength,
                        ValidationMaxLength = model.ValidationMaxLength,
                        ValidationFileAllowedExtensions = model.ValidationFileAllowedExtensions,
                        ValidationFileMaximumSize = model.ValidationFileMaximumSize,
                        DefaultValue = model.DefaultValue
                    };
                    _productAttributeService.InsertProductAttributeMapping(productAttributeMapping);

                    //Comma seprated attribute mapping list
                    attributeMappingId = attributeMappingId + productAttributeMapping.Id.ToString() + ",";

                    UpdateLocales(productAttributeMapping, model);

                    //predefined values
                    var predefinedValues = _productAttributeService.GetPredefinedProductAttributeValues(model.ProductAttributeId);
                    foreach (var predefinedValue in predefinedValues)
                    {
                        var pav = new ProductAttributeValue
                        {
                            ProductAttributeMappingId = productAttributeMapping.Id,
                            AttributeValueType = AttributeValueType.Simple,
                            Name = predefinedValue.Name,
                            PriceAdjustment = predefinedValue.PriceAdjustment,
                            PriceAdjustmentUsePercentage = predefinedValue.PriceAdjustmentUsePercentage,
                            WeightAdjustment = predefinedValue.WeightAdjustment,
                            Cost = predefinedValue.Cost,
                            IsPreSelected = predefinedValue.IsPreSelected,
                            DisplayOrder = predefinedValue.DisplayOrder
                        };
                        _productAttributeService.InsertProductAttributeValue(pav);

                        //locales
                        var languages = _languageService.GetAllLanguages(true);

                        //localization
                        foreach (var lang in languages)
                        {
                            var name = _localizationService.GetLocalized(predefinedValue, x => x.Name, lang.Id, false, false);
                            if (!string.IsNullOrEmpty(name))
                                _localizedEntityService.SaveLocalizedValue(pav, x => x.Name, name, lang.Id);
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < productAttributeMap.Count; j++)
                    {
                        //Comma seprated attribute mapping list
                        attributeMappingId = attributeMappingId + productAttributeMap[j].Id.ToString() + ",";
                    }
                }
            }

            if (!string.IsNullOrEmpty(attributeMappingId))
                attributeMappingId = attributeMappingId.Substring(0, attributeMappingId.Length - 1);

            //Insert attribute mapping with segment
            var mapper = new GBS_ProductAttributeMap()
            {
                EntityType = EntityTypeEnum.ProductAttributeMap.ToString(),
                EntityId = model.ProductAttributeId,
                SegmentId = model.ProductSegmentId,
                AttributeMapperId = attributeMappingId
            };
            _productFilterOptionService.InsertProductAttributeMapWithSegment(mapper);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.ProductAttributes.Attributes.Added"));

            if (!continueEditing)
            {
                SaveSelectedTabName("tab-ProductAttributes");
                
                return RedirectToAction("Edit", new { id = model.ProductSegmentId });
            }

            //selected tab
            SaveSelectedTabName("tab-info");

            return RedirectToAction("ProductAttributeMappingEdit", new { productSegmentId = model.ProductSegmentId, productAttributeId = model.ProductAttributeId });
        }

        public IActionResult ProductAttributeMappingEdit(int productSegmentId, int productAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            //try to get a product attribute mapping with the specified id
            var productAttributeSegmentMapping = _productFilterOptionService.GetProductAttributeMapWithSegment(productAttributeId, EntityTypeEnum.ProductAttributeMap, productSegmentId).FirstOrDefault()
                ?? throw new ArgumentException("No product attribute mapping found with the specified id");
            
            var productAttributeMapping = _productAttributeService.GetProductAttributeMappingById(productAttributeSegmentMapping.AttributeMapperIdList.FirstOrDefault())
                ?? throw new ArgumentException("No product attribute mapping found with the specified id");

            //prepare model
            var model = _segmentModelFactory.PrepareProductAttributeMappingModel(null, productSegmentId, productAttributeId, productAttributeMapping);
            model.AttributeMappedIds = productAttributeSegmentMapping.AttributeMapperId;
            model.GBS_ProductAttributeMapId = productAttributeSegmentMapping.Id;

            if (model.ProductAttributeId == 0)
                return RedirectToAction("Edit", new { id = model.ProductSegmentId });

            return View("~/Plugins/GBS.Plugin.ProductManagement/Views/ProductAttribute/ProductAttributeMappingEdit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult ProductAttributeMappingEdit(Models.ProductAttributeMappingModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            //try to get a product attribute mapping with the specified id
            var productAttributeSegmentMapping = _productFilterOptionService.GetProductAttributeMapWithSegment(model.ProductAttributeId, EntityTypeEnum.ProductAttributeMap, model.ProductSegmentId).FirstOrDefault();
            
            string attributeMappingId = "";

            int totalRecords = 0;
            var products = _productFilterOptionService.GetProductsBySegmentId(model.ProductSegmentId, out totalRecords);
            
            for (int i = 0; i < products.Count; i++)
            {
                //ensure this attribute is not mapped yet
                var productAttributeMap = _productAttributeService.GetProductAttributeMappingsByProductId(products[i].Id)
                    .Where(x=> productAttributeSegmentMapping.AttributeMapperIdList.Contains(x.Id)).ToList();
                if (productAttributeMap.Count == 0)
                {
                    //insert mapping
                    var productAttributeMapping = new ProductAttributeMapping
                    {
                        ProductId = products[i].Id,
                        ProductAttributeId = model.ProductAttributeId,
                        TextPrompt = model.TextPrompt,
                        IsRequired = model.IsRequired,
                        AttributeControlTypeId = model.AttributeControlTypeId,
                        DisplayOrder = model.DisplayOrder,
                        ValidationMinLength = model.ValidationMinLength,
                        ValidationMaxLength = model.ValidationMaxLength,
                        ValidationFileAllowedExtensions = model.ValidationFileAllowedExtensions,
                        ValidationFileMaximumSize = model.ValidationFileMaximumSize,
                        DefaultValue = model.DefaultValue
                    };
                    _productAttributeService.InsertProductAttributeMapping(productAttributeMapping);

                    //Comma seprated attribute mapping list
                    attributeMappingId = attributeMappingId + productAttributeMapping.Id.ToString() + ",";

                    UpdateLocales(productAttributeMapping, model);
                }
                else
                {

                    //try to get a product attribute mapping with the specified id
                    var productAttributeMapping = productAttributeMap.FirstOrDefault();
                    
                    productAttributeMapping.ProductAttributeId = model.ProductAttributeId;
                    productAttributeMapping.TextPrompt = model.TextPrompt;
                    productAttributeMapping.IsRequired = model.IsRequired;
                    productAttributeMapping.AttributeControlTypeId = model.AttributeControlTypeId;
                    productAttributeMapping.DisplayOrder = model.DisplayOrder;
                    productAttributeMapping.ValidationMinLength = model.ValidationMinLength;
                    productAttributeMapping.ValidationMaxLength = model.ValidationMaxLength;
                    productAttributeMapping.ValidationFileAllowedExtensions = model.ValidationFileAllowedExtensions;
                    productAttributeMapping.ValidationFileMaximumSize = model.ValidationFileMaximumSize;
                    productAttributeMapping.DefaultValue = model.DefaultValue;

                    _productAttributeService.UpdateProductAttributeMapping(productAttributeMapping);

                    //Comma seprated attribute mapping list
                    attributeMappingId = attributeMappingId + productAttributeMapping.Id.ToString() + ",";

                    UpdateLocales(productAttributeMapping, model);
                }
            }
            
            if (!string.IsNullOrEmpty(attributeMappingId))
                attributeMappingId = attributeMappingId.Substring(0, attributeMappingId.Length - 1);

            //Updte attribute mapping with segment
            productAttributeSegmentMapping.AttributeMapperId = attributeMappingId;

            _productFilterOptionService.UpdateProductAttributeMapWithSegment(productAttributeSegmentMapping);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.ProductAttributes.Attributes.Updated"));

            if (!continueEditing)
            {
                SaveSelectedTabName("tab-ProductAttributes");

                return RedirectToAction("Edit", new { id = model.ProductSegmentId });
            }

            //selected tab
            SaveSelectedTabName("tab-info");

            return RedirectToAction("ProductAttributeMappingEdit", new { productSegmentId = model.ProductSegmentId, productAttributeId = model.ProductAttributeId });
        }

        [HttpPost]
        public virtual IActionResult ProductAttributeMappingDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            var segmentMap = _productFilterOptionService.GetProductAttributeMapWithSegmentById(id);
            for (int i = 0; i < segmentMap.AttributeMapperIdList.Count; i++)
            {
                //try to get a product attribute mapping with the specified id
                var productAttributeMapping = _productAttributeService.GetProductAttributeMappingById(segmentMap.AttributeMapperIdList[i]);
                if (productAttributeMapping != null)
                {
                    //try to get a product with the specified id
                    var product = _productService.GetProductById(productAttributeMapping.ProductId);
                    if (product != null)
                        _productAttributeService.DeleteProductAttributeMapping(productAttributeMapping);
                }
            }
            var segmentValueMap = _productFilterOptionService.GetProductAttributeMapWithSegment(segmentMap.EntityId, EntityTypeEnum.ProductAttributeMapValue, segmentMap.SegmentId);
            for (int i = 0; i < segmentValueMap.Count; i++)
            {
                _productFilterOptionService.DeleteProductAttributeMapWithSegment(segmentValueMap[i]);
            }
            _productFilterOptionService.DeleteProductAttributeMapWithSegment(segmentMap);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.ProductAttributes.Attributes.Deleted"));

            //selected tab
            SaveSelectedTabName("tab-ProductAttributes");

            return RedirectToAction("Edit", new { id = segmentMap.SegmentId });
        }


        [HttpPost]
        public virtual IActionResult ProductAttributeValueList(string attributeMappedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            List<int> attributeMapIds = attributeMappedIds.Split(',').Select(int.Parse).ToList();

            if (attributeMapIds != null && attributeMapIds.Count > 0)
            {
                ProductAttributeValueSearchModel searchModel = null;
                ProductAttributeMapping productAttributeMapping = null;

                for (int i = 0; i < attributeMapIds.Count; i++)
                {
                    searchModel = new ProductAttributeValueSearchModel()
                    {
                        ProductAttributeMappingId = attributeMapIds[i]
                    };

                    //try to get a product attribute mapping with the specified id
                    productAttributeMapping = _productAttributeService.GetProductAttributeMappingById(attributeMapIds[i]);

                    if (productAttributeMapping != null)
                        break;
                }

                //prepare model
                var model = _productModelFactory.PrepareProductAttributeValueListModel(searchModel, productAttributeMapping);

                return Json(model);
            }
            else
                return null;
        }

        public virtual IActionResult ProductAttributeValueCreatePopup(int productSegmentId, int productAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            //try to get a product attribute mapping with the specified id
            var productAttributeSegmentMapping = _productFilterOptionService.GetProductAttributeMapWithSegment(productAttributeId, EntityTypeEnum.ProductAttributeMap, productSegmentId).FirstOrDefault()
                ?? throw new ArgumentException("No product attribute mapping found with the specified id");

            
            var productAttributeMapping = _productAttributeService.GetProductAttributeMappingById(productAttributeSegmentMapping.AttributeMapperIdList.FirstOrDefault())
                ?? throw new ArgumentException("No product attribute mapping found with the specified id");

            //prepare model
            var model = _segmentModelFactory.PrepareProductAttributeValueModel(new Models.ProductAttributeValueModel(), productAttributeMapping, null);
            model.ProductSegmentId = productSegmentId;
            model.AttributeMappedIds = productAttributeSegmentMapping.AttributeMapperId;
            model.ProductAttributeId = productAttributeId;
            
            return View("~/Plugins/GBS.Plugin.ProductManagement/Views/ProductAttribute/ProductAttributeValueCreatePopup.cshtml", model);
        }

        [HttpPost]
        public virtual IActionResult ProductAttributeValueCreatePopup(Models.ProductAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            List<int> attributeMapIds = model.AttributeMappedIds.Split(',').Select(int.Parse).ToList();

            string attributeValuesId = "";
            for (int i = 0; i < attributeMapIds.Count; i++)
            {
                //try to get a product attribute mapping with the specified id
                var productAttributeMapping = _productAttributeService.GetProductAttributeMappingById(attributeMapIds[i]);
                if (productAttributeMapping == null)
                    return RedirectToAction("Edit", new { id = model.ProductSegmentId });

                if (productAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares)
                {
                    //ensure valid color is chosen/entered
                    if (string.IsNullOrEmpty(model.ColorSquaresRgb))
                        ModelState.AddModelError(string.Empty, "Color is required");
                    try
                    {
                        //ensure color is valid (can be instantiated)
                        System.Drawing.ColorTranslator.FromHtml(model.ColorSquaresRgb);
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.Message);
                    }
                }

                //ensure a picture is uploaded
                if (productAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares && model.ImageSquaresPictureId == 0)
                {
                    ModelState.AddModelError(string.Empty, "Image is required");
                }

                var pav = new ProductAttributeValue
                {
                    ProductAttributeMappingId = attributeMapIds[i],
                    AttributeValueTypeId = model.AttributeValueTypeId,
                    AssociatedProductId = model.AssociatedProductId,
                    Name = model.Name,
                    ColorSquaresRgb = model.ColorSquaresRgb,
                    ImageSquaresPictureId = model.ImageSquaresPictureId,
                    PriceAdjustment = model.PriceAdjustment,
                    PriceAdjustmentUsePercentage = model.PriceAdjustmentUsePercentage,
                    WeightAdjustment = model.WeightAdjustment,
                    Cost = model.Cost,
                    CustomerEntersQty = model.CustomerEntersQty,
                    Quantity = model.CustomerEntersQty ? 1 : model.Quantity,
                    IsPreSelected = model.IsPreSelected,
                    DisplayOrder = model.DisplayOrder,
                    PictureId = model.PictureId
                };

                _productAttributeService.InsertProductAttributeValue(pav);
                UpdateLocales(pav, model);
                attributeValuesId = attributeValuesId + pav.Id + ",";
            }

            if (!string.IsNullOrEmpty(attributeValuesId))
                attributeValuesId = attributeValuesId.Substring(0, attributeValuesId.Length - 1);

            //Insert attribute mapping with segment
            var mapper = new GBS_ProductAttributeMap()
            {
                EntityType = EntityTypeEnum.ProductAttributeMapValue.ToString(),
                EntityId = model.ProductAttributeId,
                SegmentId = model.ProductSegmentId,
                AttributeMapperId = attributeValuesId
            };
            _productFilterOptionService.InsertProductAttributeMapWithSegment(mapper);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.ProductAttributes.Attributes.Added"));
            ViewBag.RefreshPage = true;
            return View("~/Plugins/GBS.Plugin.ProductManagement/Views/ProductAttribute/ProductAttributeValueCreatePopup.cshtml", model);
        }

        public virtual IActionResult ProductAttributeValueEditPopup(int id, int productSegmentId, int productAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            //try to get a product attribute mapping with the specified id
            var productAttributeSegmentMapping = _productFilterOptionService.GetProductAttributeMapWithSegmentByAttributeMapperId(id,productAttributeId, EntityTypeEnum.ProductAttributeMapValue, productSegmentId)
                ?? throw new ArgumentException("No product attribute mapping found with the specified id");

            
            //try to get a product attribute value with the specified id
            var productAttributeValue = _productAttributeService.GetProductAttributeValueById(productAttributeSegmentMapping.AttributeMapperIdList.FirstOrDefault());
            if (productAttributeValue == null)
                return RedirectToAction("Edit", "ProductSegment", new { id = productSegmentId });

            
            var productAttributeMapping = _productAttributeService.GetProductAttributeMappingById(productAttributeValue.ProductAttributeMappingId)
                ?? throw new ArgumentException("No product attribute mapping found with the specified id");
            
            //prepare model
            var model = _segmentModelFactory.PrepareProductAttributeValueModel(null, productAttributeMapping, productAttributeValue);
            model.ProductSegmentId = productSegmentId;
            model.AttributeMappedIds = productAttributeSegmentMapping.AttributeMapperId;
            model.ProductAttributeId = productAttributeId;
            model.GBS_ProductAttributeMapId = productAttributeSegmentMapping.Id;

            return View("~/Plugins/GBS.Plugin.ProductManagement/Views/ProductAttribute/ProductAttributeValueEditPopup.cshtml", model);
        }

        [HttpPost]
        public virtual IActionResult ProductAttributeValueEditPopup(Models.ProductAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            List<int> attributeValueMapIds = model.AttributeMappedIds.Split(',').Select(int.Parse).ToList();

            var productAttributeSegmentMapping = _productFilterOptionService.GetProductAttributeMapWithSegment(model.ProductAttributeId, EntityTypeEnum.ProductAttributeMap, model.ProductSegmentId).FirstOrDefault();

            string attributeValuesId = "";
            for (int i = 0; i < productAttributeSegmentMapping.AttributeMapperIdList.Count; i++)
            {
                //try to get a product attribute mapping with the specified id
                var productAttributeMapping = _productAttributeService.GetProductAttributeMappingById(productAttributeSegmentMapping.AttributeMapperIdList[i]);
                if (productAttributeMapping == null)
                    return RedirectToAction("Edit", new { id = model.ProductSegmentId });

                var productAttributeValue = productAttributeMapping.ProductAttributeValues.Where(p => attributeValueMapIds.Contains(p.Id)).FirstOrDefault();
                if (productAttributeValue != null)
                {
                    if (productAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares)
                    {
                        //ensure valid color is chosen/entered
                        if (string.IsNullOrEmpty(model.ColorSquaresRgb))
                            ModelState.AddModelError(string.Empty, "Color is required");
                        try
                        {
                            //ensure color is valid (can be instantiated)
                            System.Drawing.ColorTranslator.FromHtml(model.ColorSquaresRgb);
                        }
                        catch (Exception exc)
                        {
                            ModelState.AddModelError(string.Empty, exc.Message);
                        }
                    }

                    //ensure a picture is uploaded
                    if (productAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares && model.ImageSquaresPictureId == 0)
                    {
                        ModelState.AddModelError(string.Empty, "Image is required");
                    }

                    productAttributeValue.ProductAttributeMappingId = productAttributeSegmentMapping.AttributeMapperIdList[i];
                    productAttributeValue.AttributeValueTypeId = model.AttributeValueTypeId;
                    productAttributeValue.AssociatedProductId = model.AssociatedProductId;
                    productAttributeValue.Name = model.Name;
                    productAttributeValue.ColorSquaresRgb = model.ColorSquaresRgb;
                    productAttributeValue.ImageSquaresPictureId = model.ImageSquaresPictureId;
                    productAttributeValue.PriceAdjustment = model.PriceAdjustment;
                    productAttributeValue.PriceAdjustmentUsePercentage = model.PriceAdjustmentUsePercentage;
                    productAttributeValue.WeightAdjustment = model.WeightAdjustment;
                    productAttributeValue.Cost = model.Cost;
                    productAttributeValue.CustomerEntersQty = model.CustomerEntersQty;
                    productAttributeValue.Quantity = model.CustomerEntersQty ? 1 : model.Quantity;
                    productAttributeValue.IsPreSelected = model.IsPreSelected;
                    productAttributeValue.DisplayOrder = model.DisplayOrder;
                    productAttributeValue.PictureId = model.PictureId;

                    _productAttributeService.UpdateProductAttributeValue(productAttributeValue);
                    UpdateLocales(productAttributeValue, model);
                    attributeValuesId = attributeValuesId + productAttributeValue.Id + ",";
                }
                else
                {
                    if (productAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares)
                    {
                        //ensure valid color is chosen/entered
                        if (string.IsNullOrEmpty(model.ColorSquaresRgb))
                            ModelState.AddModelError(string.Empty, "Color is required");
                        try
                        {
                            //ensure color is valid (can be instantiated)
                            System.Drawing.ColorTranslator.FromHtml(model.ColorSquaresRgb);
                        }
                        catch (Exception exc)
                        {
                            ModelState.AddModelError(string.Empty, exc.Message);
                        }
                    }

                    //ensure a picture is uploaded
                    if (productAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares && model.ImageSquaresPictureId == 0)
                    {
                        ModelState.AddModelError(string.Empty, "Image is required");
                    }

                    var pav = new ProductAttributeValue
                    {
                        ProductAttributeMappingId = productAttributeSegmentMapping.AttributeMapperIdList[i],
                        AttributeValueTypeId = model.AttributeValueTypeId,
                        AssociatedProductId = model.AssociatedProductId,
                        Name = model.Name,
                        ColorSquaresRgb = model.ColorSquaresRgb,
                        ImageSquaresPictureId = model.ImageSquaresPictureId,
                        PriceAdjustment = model.PriceAdjustment,
                        PriceAdjustmentUsePercentage = model.PriceAdjustmentUsePercentage,
                        WeightAdjustment = model.WeightAdjustment,
                        Cost = model.Cost,
                        CustomerEntersQty = model.CustomerEntersQty,
                        Quantity = model.CustomerEntersQty ? 1 : model.Quantity,
                        IsPreSelected = model.IsPreSelected,
                        DisplayOrder = model.DisplayOrder,
                        PictureId = model.PictureId
                    };

                    _productAttributeService.InsertProductAttributeValue(pav);
                    UpdateLocales(pav, model);
                    attributeValuesId = attributeValuesId + pav.Id + ",";
                }
            }
            
            if (!string.IsNullOrEmpty(attributeValuesId))
                attributeValuesId = attributeValuesId.Substring(0, attributeValuesId.Length - 1);

            var mapper = _productFilterOptionService.GetProductAttributeMapWithSegmentById(model.GBS_ProductAttributeMapId);
            //Update attribute mapping with segment
            mapper.AttributeMapperId = attributeValuesId;

            _productFilterOptionService.UpdateProductAttributeMapWithSegment(mapper);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.ProductAttributes.Attributes.Added"));
            ViewBag.RefreshPage = true;
            return View("~/Plugins/GBS.Plugin.ProductManagement/Views/ProductAttribute/ProductAttributeValueCreatePopup.cshtml", model);
        }

        [HttpPost]
        public virtual IActionResult ProductAttributeValueDelete(int id, int productSegmentId, int productAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            //try to get a product attribute mapping with the specified id
            var productAttributeSegmentMapping = _productFilterOptionService.GetProductAttributeMapWithSegmentByAttributeMapperId(id, productAttributeId, EntityTypeEnum.ProductAttributeMapValue, productSegmentId);

            for (int i = 0; i < productAttributeSegmentMapping.AttributeMapperIdList.Count; i++)
            {
                //try to get a product attribute value with the specified id
                var productAttributeValue = _productAttributeService.GetProductAttributeValueById(productAttributeSegmentMapping.AttributeMapperIdList[i]);

                if (productAttributeValue != null)
                {
                    //try to get a product attribute mapping with the specified id
                    var productAttributeMapping = _productAttributeService.GetProductAttributeMappingById(productAttributeValue.ProductAttributeMappingId);

                    if (productAttributeMapping != null)
                    {
                        //try to get a product with the specified id
                        var product = _productService.GetProductById(productAttributeMapping.ProductId);

                        if (product != null)
                            _productAttributeService.DeleteProductAttributeValue(productAttributeValue);
                    }
                }
            }

            _productFilterOptionService.DeleteProductAttributeMapWithSegment(productAttributeSegmentMapping);

            return new NullJsonResult();
        }
        #endregion

        #region Products specification attribute
        [HttpPost]
        public virtual IActionResult ProductSpecificationAttributeList(DataSourceRequest command, int productSegmentId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            int totalRecords = 0;
            var productSpecificationAttributes = _productFilterOptionService.GetProductSpecificationAttributeBySegmentId(productSegmentId, out totalRecords, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = productSpecificationAttributes.Select(productSpecificationAttribute => new ProductSpecificationAttributes
                {
                    Id = productSpecificationAttribute.Id,
                    Name = productSpecificationAttribute.Name
                }),
                Total = totalRecords
            };

            return Json(gridModel);
        }
        #endregion

        #endregion
    }
}

