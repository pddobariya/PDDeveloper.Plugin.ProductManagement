using PDDeveloper.Plugin.ProductManagement.Domain;
using PDDeveloper.Plugin.ProductManagement.Domain.Enums;
using PDDeveloper.Plugin.ProductManagement.Factories;
using PDDeveloper.Plugin.ProductManagement.Models;
using PDDeveloper.Plugin.ProductManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
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
using Nop.Web.Areas.Admin.Infrastructure.Cache;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Services.Messages;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Net;
using Nop.Web.Framework.Factories;

namespace PDDeveloper.Plugin.ProductManagement.Controllers
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
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ILocalizedModelFactory _localizedModelFactory;
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
            IProductModelFactory productModelFactory,
            ISpecificationAttributeService specificationAttributeService,
            IWorkContext workContext,
            INotificationService notificationService,
            IBaseAdminModelFactory baseAdminModelFactory,
            ILocalizedModelFactory localizedModelFactory)
        {
            _permissionService = permissionService;
            _segmentModelFactory = segmentModelFactory;
            _productSegmentService = productSegmentService;
            _localizationService = localizationService;
            _customerActivityService = customerActivityService;
            _productFilterOptionService = productFilterOptionService;
            _productService = productService;
            _categoryService = categoryService;
            _cacheManager = cacheManager;
            _manufacturerService = manufacturerService;
            _storeService = storeService;
            _vendorService = vendorService;
            _productAttributeService = productAttributeService;
            _languageService = languageService;
            _localizedEntityService = localizedEntityService;
            _productModelFactory = productModelFactory;
            _specificationAttributeService = specificationAttributeService;
            _workContext = workContext;
            _notificationService = notificationService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _localizedModelFactory = localizedModelFactory;
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

        /// <summary>
        /// Prepare specification attribute model to add to the product
        /// </summary>
        /// <param name="model">Specification attribute model to add to the product</param>
        /// <returns>Specification attribute model to add to the product</returns>
        protected virtual ProductSegmentAddSpecificationAttributeModel PrepareAddSpecificationAttributeToProductModel(
            ProductSegmentAddSpecificationAttributeModel model, int productSpecificationId,int? id)
        {
            Action<AddSpecificationAttributeLocalizedModel, int> localizedModelConfiguration = null;

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (!id.HasValue)
            {
                model.ShowOnProductPage = true;

                var attribute = _specificationAttributeService.GetSpecificationAttributeById(productSpecificationId);

                if (attribute != null)
                {
                    //options of preselected specification attribute
                    model.AvailableOptions = _specificationAttributeService
                        .GetSpecificationAttributeOptionsBySpecificationAttribute(attribute.Id)
                        .Select(option => new SelectListItem { Text = option.Name, Value = option.Id.ToString() }).ToList();

                    model.SpecificationAttributeName = attribute != null ? attribute.Name : "";
                }
                model.SpecificationAttributeId = productSpecificationId;
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

                return model;
            }
            else
            {
                var attribute = _specificationAttributeService.GetProductSpecificationAttributeById(id.Value);

                if (attribute == null)
                {
                    throw new ArgumentException("No specification attribute found with the specified id");
                }

                //a vendor should have access only to his products
                if (_workContext.CurrentVendor != null && attribute.Product.VendorId != _workContext.CurrentVendor.Id)
                    throw new UnauthorizedAccessException("This is not your product");

                model.AttributeTypeId = attribute.AttributeTypeId;
                model.ProductId = attribute.ProductId;
                model.AllowFiltering = attribute.AllowFiltering;
                model.ShowOnProductPage = attribute.ShowOnProductPage;
                model.DisplayOrder = attribute.DisplayOrder;
                model.SpecificationAttributeOptionId = attribute.SpecificationAttributeOptionId;

                model.SpecificationId = attribute.Id;
                model.AttributeId = attribute.SpecificationAttributeOption.SpecificationAttribute.Id;
                model.AttributeTypeName = _localizationService.GetLocalizedEnum(attribute.AttributeType);
                model.AttributeName = attribute.SpecificationAttributeOption.SpecificationAttribute.Name;
                model.SpecificationAttributeName = attribute.SpecificationAttributeOption.SpecificationAttribute.Name;

                model.AvailableAttributes = _cacheManager.Get(NopModelCacheDefaults.SpecAttributesModelKey, () =>
                {
                    return _specificationAttributeService.GetSpecificationAttributesWithOptions()
                        .Select(attributeWithOption => new SelectListItem(attributeWithOption.Name, attributeWithOption.Id.ToString()))
                        .ToList();
                });

                model.AvailableOptions = _specificationAttributeService
                    .GetSpecificationAttributeOptionsBySpecificationAttribute(model.AttributeId)
                    .Select(option => new SelectListItem { Text = option.Name, Value = option.Id.ToString() })
                    .ToList();

                switch (attribute.AttributeType)
                {
                    case SpecificationAttributeType.Option:
                        model.ValueRaw = WebUtility.HtmlEncode(attribute.SpecificationAttributeOption.Name);
                        model.SpecificationAttributeOptionId = attribute.SpecificationAttributeOptionId;
                        break;
                    case SpecificationAttributeType.CustomText:
                        model.Value = WebUtility.HtmlDecode(attribute.CustomValue);
                        break;
                    case SpecificationAttributeType.CustomHtmlText:
                        model.ValueRaw = attribute.CustomValue;
                        break;
                    case SpecificationAttributeType.Hyperlink:
                        model.Value = attribute.CustomValue;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(attribute.AttributeType));
                }

                localizedModelConfiguration = (locale, languageId) =>
                {
                    switch (attribute.AttributeType)
                    {
                        case SpecificationAttributeType.CustomHtmlText:
                            locale.ValueRaw = _localizationService.GetLocalized(attribute, entity => entity.CustomValue, languageId, false, false);
                            break;
                        case SpecificationAttributeType.CustomText:
                            locale.Value = _localizationService.GetLocalized(attribute, entity => entity.CustomValue, languageId, false, false);
                            break;
                        case SpecificationAttributeType.Option:
                            break;
                        case SpecificationAttributeType.Hyperlink:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                };

                model.SpecificationAttributeId = productSpecificationId;
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

                return model;
            }
        }

        /// <summary>
        /// Prepare product specification attribute search model
        /// </summary>
        /// <param name="searchModel">Product specification attribute search model</param>
        /// <param name="product">Product</param>
        /// <returns>Product specification attribute search model</returns>
        protected virtual ProductSpecificationAttributeSearchModel PrepareProductSpecificationAttributeSearchModel(
            ProductSpecificationAttributeSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
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

            return View("~/Plugins/PDDeveloper.Plugin.ProductManagement/Views/ProductSegment/List.cshtml", model);
        }

        [HttpPost]
        public virtual IActionResult List(ProductSegmentSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedDataTablesJson();

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
            var model = _segmentModelFactory.PrepareProductSegmentModel(new SegmentModel(), null);

            return View(ProductManagementDefaults.AdminViewPath + "ProductSegment/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Create(SegmentModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var productSegment = new PDD_ProductSegment
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
                    string.Format(_localizationService.GetResource("Plugins.PDD.ProductManagement.Segment.ActivityLog.CreateSegment"), productSegment.Name), productSegment);

                _notificationService.SuccessNotification(_localizationService.GetResource("Plugins.PDD.ProductManagement.Segment.Added"));

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

            return View(ProductManagementDefaults.AdminViewPath + "ProductSegment/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Edit(SegmentModel model, bool continueEditing)
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
                    string.Format(_localizationService.GetResource("Plugins.PDD.ProductManagement.Segment.ActivityLog.EditSegment"), productSegment.Name), productSegment);

                _notificationService.SuccessNotification(_localizationService.GetResource("Plugins.PDD.ProductManagement.Segment.Updated"));

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
                string.Format(_localizationService.GetResource("Plugins.PDD.ProductManagement.Segment.ActivityLog.DeletedSegment"), segment.Name), segment);

            return new NullJsonResult();
        }
        #endregion

        #endregion

        #region ProductSegment Opction
        [HttpPost]
        public virtual IActionResult ProductSegmentOpctionList(ProductSegmentOptionSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedDataTablesJson();


            //prepare model
            var segmentOpctions = _productFilterOptionService.GetAllFilterOptionBySegmentId(searchModel.ProductSegmentId).ToPagedList(searchModel);

            var model = new ProductSegmentOptionListModel().PrepareToGrid(searchModel, segmentOpctions, () =>
            {
                //fill in model values from the entity
                return segmentOpctions.Select(segmentOpction =>
                {
                    var segmentOpctionModel = segmentOpction.ToModel<ProductFilterOptionsModel>();

                    return segmentOpctionModel;
                });
            });

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult ProductFilterOptionUpdate(ProductFilterOptionsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return ErrorJson(ModelState.SerializeErrors());

            var segmentOpction = _productFilterOptionService.GetProductFilterOptionById(model.Id);

            //fill entity from model
            segmentOpction = model.ToEntity(segmentOpction);
            segmentOpction.UpdatedOnUtc = DateTime.UtcNow;

            _productFilterOptionService.UpdateProductFilterOption(segmentOpction);

            return Json(new { Result = true });
        }

        [HttpPost]
        public virtual IActionResult ProductFilterOptionAdd(int productSegmentId, ProductFilterOptionsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return ErrorJson(ModelState.SerializeErrors());

            //fill entity from model
            var segmentOpction = model.ToEntity<PDD_ProductFilterOptions>();
            segmentOpction.ProductSegmentManagerId = productSegmentId;
            segmentOpction.CreatedOnUtc = DateTime.UtcNow;
            segmentOpction.UpdatedOnUtc = DateTime.UtcNow;

            _productFilterOptionService.InsertProductFilterOption(segmentOpction);

            return Json(new { Result = true });
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

            return Json(new { Result = true });
        }

        #endregion

        #region IncludeExclude-products

        [HttpPost]
        public virtual IActionResult IncludeExcludeProductList(IncludeExcludeProductSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedDataTablesJson();

            var includeproducts = _productFilterOptionService.GetAllIncludeExcludeProductBySegmentId(searchModel.ProductSegmentId, searchModel.ProductType).ToPagedList(searchModel);

            //prepare grid model
            var model = new IncludeExcludeProductListModel().PrepareToGrid(searchModel, includeproducts, () =>
            {
                return includeproducts.Select(includeproduct =>
                {
                    //fill in model values from the entity
                    var includeproductModel = new IncludeExcludeProductModel
                    {
                        Id = includeproduct.Id,
                        ProductSegmentId = searchModel.ProductSegmentId,
                        ProductId = includeproduct.ProductId,
                        ProductType = ((SegmentProductType)includeproduct.ProductType).ToString(),
                        ProductName = _productService.GetProductById(includeproduct.ProductId)?.Name,
                    };

                    return includeproductModel;
                });
            });

            return Json(model);
        }

        public virtual IActionResult ProductDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //try to get a product mapped with segment with the specified id
            var incExc = _productFilterOptionService.GetIncludeExcludeProductById(id);
            if (incExc == null)
                throw new ArgumentException("No widget content mapping found with the specified id");

            _productFilterOptionService.DeleteIncludeExcludeProduct(incExc);

            return new NullJsonResult();
        }

        public virtual IActionResult ProductAddPopup(int productSegmentId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var searchModel = new IncludeExcludeProductSearchModel();

            //prepare available categories
            _baseAdminModelFactory.PrepareCategories(searchModel.AvailableCategories);

            //prepare available manufacturers
            _baseAdminModelFactory.PrepareManufacturers(searchModel.AvailableManufacturers);

            //prepare available stores
            _baseAdminModelFactory.PrepareStores(searchModel.AvailableStores);

            //prepare available vendors
            _baseAdminModelFactory.PrepareVendors(searchModel.AvailableVendors);

            //prepare available product types
            _baseAdminModelFactory.PrepareProductTypes(searchModel.AvailableProductTypes);

            return View(ProductManagementDefaults.AdminViewPath + "ProductSegment/ProductAddPopup.cshtml", searchModel);
        }

        [HttpPost]
        public virtual IActionResult ProductAddPopupList(IncludeExcludeProductSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //get products
            var products = _productService.SearchProducts(showHidden: true,
                categoryIds: new List<int> { searchModel.SearchCategoryId },
                manufacturerId: searchModel.SearchManufacturerId,
                storeId: searchModel.SearchStoreId,
                vendorId: searchModel.SearchVendorId,
                productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = new AddProductToSegmentListModel().PrepareToGrid(searchModel, products, () =>
            {
                return products.Select(product =>
                {
                    var productModel = product.ToModel<ProductModel>();
                    return productModel;
                });
            });

            return Json(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual IActionResult ProductAddPopup(int productSegmentId, string btnId, string formId, int productType, IncludeExcludeProductSearchModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

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
                                new PDD_Product_Include_Exclude
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
            return View(ProductManagementDefaults.AdminViewPath + "ProductSegment/ProductAddPopup.cshtml", model);
        }

        #endregion

        #region Products

        [HttpPost]
        public virtual IActionResult ProductList(ProductSegmentSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var vendorId = _workContext.CurrentVendor != null ? _workContext.CurrentVendor.Id : 0;
            int totalRecords = 0;
            var products = _productFilterOptionService.GetProductsBySegmentId(searchModel.ProductSegmentId, out totalRecords, searchModel.Page - 1, searchModel.PageSize, vendorId).ToPagedList(searchModel);

            //prepare grid model
            var model = new SegmentProductListModel().PrepareToGrid(searchModel, products, () =>
            {
                return products.Select(product =>
                {
                    //fill in model values from the entity
                    return new SegmentProducts
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Sku = product.Sku
                    };
                });
            });

            return Json(model);
        }

        public virtual IActionResult ProductExclude(int productSegmentId, int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            _productFilterOptionService.InsertIncludeExcludeProduct(
                    new PDD_Product_Include_Exclude
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
        public virtual IActionResult ProductAttributeList(SegmentProductAttributeSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            int totalRecords = 0;
            var vendorId = _workContext.CurrentVendor != null ? _workContext.CurrentVendor.Id : 0;
            var productAttributes = _productFilterOptionService.GetProductAttributeBySegmentId(searchModel.ProductSegmentId, out totalRecords, searchModel.Page - 1, searchModel.PageSize, vendorId).ToPagedList(searchModel);

            //prepare grid model
            var model = new ProductAttributesListModel().PrepareToGrid(searchModel, productAttributes, () =>
            {
                return productAttributes.Select(productAttribute =>
                {
                    //fill in model values from the entity
                    return new ProductAttributes
                    {
                        Id = productAttribute.Id,
                        Name = productAttribute.Name,
                        isAttributeAdded = _productFilterOptionService.GetProductAttributeMapWithSegment(productAttribute.Id, EntityTypeEnum.ProductAttributeMap, searchModel.ProductSegmentId).Count() > 0
                    };
                });
            });

            return Json(model);
        }

        public IActionResult ProductAttributeMappingCreate(int productSegmentId, int productAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //prepare model
            var model = _segmentModelFactory.PrepareProductAttributeMappingModel(new Models.ProductAttributeMappingModel(), productSegmentId, productAttributeId, null);

            if (model.ProductAttributeId == 0)
                return RedirectToAction("Edit", new { id = model.ProductSegmentId });

            return View(ProductManagementDefaults.AdminViewPath + "ProductAttribute/ProductAttributeMappingCreate.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult ProductAttributeMappingCreate(Models.ProductAttributeMappingModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var productAttributeSegmentMapping = _productFilterOptionService.GetProductAttributeMapWithSegment(model.ProductAttributeId, EntityTypeEnum.ProductAttributeMap, model.ProductSegmentId);
            if (productAttributeSegmentMapping != null && productAttributeSegmentMapping.Count > 0)
            {
                //redisplay form
                _notificationService.ErrorNotification(_localizationService.GetResource("Admin.Catalog.Products.ProductAttributes.Attributes.AlreadyExists"));

                model = _segmentModelFactory.PrepareProductAttributeMappingModel(model, model.ProductAttributeId, model.ProductSegmentId, null, true);

                return View(ProductManagementDefaults.AdminViewPath + "ProductAttribute/ProductAttributeMappingCreate.cshtml", model);
            }

            var vendorId = _workContext.CurrentVendor != null ? _workContext.CurrentVendor.Id : 0;
            int totalRecords = 0;
            var products = _productFilterOptionService.GetProductsBySegmentId(model.ProductSegmentId, out totalRecords, vendorId: vendorId);

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
            var mapper = new PDD_ProductAttributeMap()
            {
                EntityType = EntityTypeEnum.ProductAttributeMap.ToString(),
                EntityId = model.ProductAttributeId,
                SegmentId = model.ProductSegmentId,
                AttributeMapperId = attributeMappingId
            };
            _productFilterOptionService.InsertProductAttributeMapWithSegment(mapper);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.ProductAttributes.Attributes.Added"));

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
                return AccessDeniedView();

            //try to get a product attribute mapping with the specified id
            var productAttributeSegmentMapping = _productFilterOptionService.GetProductAttributeMapWithSegment(productAttributeId, EntityTypeEnum.ProductAttributeMap, productSegmentId).FirstOrDefault()
                ?? throw new ArgumentException("No product attribute mapping found with the specified id");

            var productAttributeMapping = _productAttributeService.GetProductAttributeMappingById(productAttributeSegmentMapping.AttributeMapperIdList.FirstOrDefault())
                ?? throw new ArgumentException("No product attribute mapping found with the specified id");

            //prepare model
            var model = _segmentModelFactory.PrepareProductAttributeMappingModel(null, productSegmentId, productAttributeId, productAttributeMapping);
            model.AttributeMappedIds = productAttributeSegmentMapping.AttributeMapperId;
            model.PDD_ProductAttributeMapId = productAttributeSegmentMapping.Id;

            if (model.ProductAttributeId == 0)
                return RedirectToAction("Edit", new { id = model.ProductSegmentId });

            return View(ProductManagementDefaults.AdminViewPath + "ProductAttribute/ProductAttributeMappingEdit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult ProductAttributeMappingEdit(Models.ProductAttributeMappingModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //try to get a product attribute mapping with the specified id
            var productAttributeSegmentMapping = _productFilterOptionService.GetProductAttributeMapWithSegment(model.ProductAttributeId, EntityTypeEnum.ProductAttributeMap, model.ProductSegmentId).FirstOrDefault();

            string attributeMappingId = "";

            var vendorId = _workContext.CurrentVendor != null ? _workContext.CurrentVendor.Id : 0;
            int totalRecords = 0;
            var products = _productFilterOptionService.GetProductsBySegmentId(model.ProductSegmentId, out totalRecords, vendorId: vendorId);

            for (int i = 0; i < products.Count; i++)
            {
                //ensure this attribute is not mapped yet
                var productAttributeMap = _productAttributeService.GetProductAttributeMappingsByProductId(products[i].Id)
                    .Where(x => productAttributeSegmentMapping.AttributeMapperIdList.Contains(x.Id)).ToList();
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

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.ProductAttributes.Attributes.Updated"));

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

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.ProductAttributes.Attributes.Deleted"));

            //selected tab
            SaveSelectedTabName("tab-ProductAttributes");

            return RedirectToAction("Edit", new { id = segmentMap.SegmentId });
        }


        [HttpPost]
        public virtual IActionResult ProductAttributeValueList(Models.ProductAttributeValueSearchModel productAttributeValueSearchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (string.IsNullOrEmpty(productAttributeValueSearchModel.AttributeMappedIds))
                return Json(new ProductAttributeValueListModel());

            var attributeMapIds = productAttributeValueSearchModel.AttributeMappedIds.Split(',').Select(int.Parse).ToList();

            if (attributeMapIds != null && attributeMapIds.Count > 0)
            {
                Models.ProductAttributeValueSearchModel searchModel = null;
                ProductAttributeMapping productAttributeMapping = null;

                for (int i = 0; i < attributeMapIds.Count; i++)
                {
                    searchModel = new Models.ProductAttributeValueSearchModel()
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
                return Json(new ProductAttributeValueListModel());
        }

        public virtual IActionResult ProductAttributeValueCreatePopup(int productSegmentId, int productAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

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

            return View(ProductManagementDefaults.AdminViewPath + "ProductAttribute/ProductAttributeValueCreatePopup.cshtml", model);
        }

        [HttpPost]
        public virtual IActionResult ProductAttributeValueCreatePopup(Models.ProductAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var attributeMapIds = model.AttributeMappedIds.Split(',').Select(int.Parse).ToList();

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
            var mapper = new PDD_ProductAttributeMap()
            {
                EntityType = EntityTypeEnum.ProductAttributeMapValue.ToString(),
                EntityId = model.ProductAttributeId,
                SegmentId = model.ProductSegmentId,
                AttributeMapperId = attributeValuesId
            };
            _productFilterOptionService.InsertProductAttributeMapWithSegment(mapper);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.ProductAttributes.Attributes.Added"));
            ViewBag.RefreshPage = true;
            return View(ProductManagementDefaults.AdminViewPath + "ProductAttribute/ProductAttributeValueCreatePopup.cshtml", model);
        }

        public virtual IActionResult ProductAttributeValueEditPopup(int id, int productSegmentId, int productAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //try to get a product attribute mapping with the specified id
            var productAttributeSegmentMapping = _productFilterOptionService.GetProductAttributeMapWithSegmentByAttributeMapperId(id, productAttributeId, EntityTypeEnum.ProductAttributeMapValue, productSegmentId);


            //try to get a product attribute value with the specified id
            var productAttributeValue = _productAttributeService.GetProductAttributeValueById(id);
            if (productAttributeValue == null)
                return RedirectToAction("Edit", "ProductSegment", new { id = productSegmentId });


            var productAttributeMapping = _productAttributeService.GetProductAttributeMappingById(productAttributeValue.ProductAttributeMappingId)
                ?? throw new ArgumentException("No product attribute mapping found with the specified id");

            //prepare model
            var model = _segmentModelFactory.PrepareProductAttributeValueModel(null, productAttributeMapping, productAttributeValue);
            model.ProductSegmentId = productSegmentId;
            model.AttributeMappedIds = productAttributeSegmentMapping != null ? productAttributeSegmentMapping.AttributeMapperId : id.ToString();
            model.ProductAttributeId = productAttributeId;
            model.PDD_ProductAttributeMapId = productAttributeSegmentMapping != null? productAttributeSegmentMapping.Id : 0;

            return View(ProductManagementDefaults.AdminViewPath + "ProductAttribute/ProductAttributeValueEditPopup.cshtml", model);
        }

        [HttpPost]
        public virtual IActionResult ProductAttributeValueEditPopup(Models.ProductAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var attributeValueMapIds = model.AttributeMappedIds != null? model.AttributeMappedIds.Split(',').Select(int.Parse).ToList(): new List<int>();

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

            if (model.PDD_ProductAttributeMapId > 0)
            {
                var mapper = _productFilterOptionService.GetProductAttributeMapWithSegmentById(model.PDD_ProductAttributeMapId);
                //Update attribute mapping with segment
                mapper.AttributeMapperId = attributeValuesId;

                _productFilterOptionService.UpdateProductAttributeMapWithSegment(mapper);
            }
            else
            {
                //Insert attribute mapping with segment
                var mapper = new PDD_ProductAttributeMap()
                {
                    EntityType = EntityTypeEnum.ProductAttributeMapValue.ToString(),
                    EntityId = model.ProductAttributeId,
                    SegmentId = model.ProductSegmentId,
                    AttributeMapperId = attributeValuesId
                };
                _productFilterOptionService.InsertProductAttributeMapWithSegment(mapper);
            }
            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.ProductAttributes.Attributes.Added"));
            ViewBag.RefreshPage = true;
            return View(ProductManagementDefaults.AdminViewPath + "ProductAttribute/ProductAttributeValueCreatePopup.cshtml", model);
        }

        [HttpPost]
        public virtual IActionResult ProductAttributeValueDelete(int id, Models.ProductAttributeValueSearchModel productAttributeValueSearchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //try to get a product attribute mapping with the specified id
            var productAttributeSegmentMapping = _productFilterOptionService.GetProductAttributeMapWithSegmentByAttributeMapperId(id, productAttributeValueSearchModel.ProductAttributeId, EntityTypeEnum.ProductAttributeMapValue, productAttributeValueSearchModel.ProductSegmentId);
            if (productAttributeSegmentMapping != null)
            {
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
            }

            _productFilterOptionService.DeleteProductAttributeMapWithSegment(productAttributeSegmentMapping);

            return new NullJsonResult();
        }
        #endregion

        #region Products specification attribute
        [HttpPost]
        public virtual IActionResult ProductSpecificationAttributeList(ProductSegmentSearchModel productSegmentSearchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            var vendorId = _workContext.CurrentVendor != null ? _workContext.CurrentVendor.Id : 0;
            int totalRecords = 0;
            var productSpecificationAttributes = _productFilterOptionService.GetProductSpecificationAttributeBySegmentId(productSegmentSearchModel.ProductSegmentId, out totalRecords, productSegmentSearchModel.Page - 1, productSegmentSearchModel.PageSize, vendorId).ToPagedList(productSegmentSearchModel);

            //prepare grid model
            var model = new Models.ProductSpecificationAttributeListModel().PrepareToGrid(productSegmentSearchModel, productSpecificationAttributes, () =>
            {
                return productSpecificationAttributes.Select(productAttribute =>
                {
                    //fill in model values from the entity
                    return new Models.ProductSpecificationAttributeModel
                    {
                        Id = productAttribute.Id,
                        AttributeName = productAttribute.Name
                    };
                });
            });

            return Json(model);
        }

        public virtual IActionResult AddSpecificationAttributeToProduct(int productSegmentId, int productSpecificationId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = new Models.ProductSpecificationAttributeModel();

            //prepare specification attribute model to add to the product
            //PrepareAddSpecificationAttributeToProductModel(model, productSpecificationId);

            PrepareProductSpecificationAttributeSearchModel(model.ProductSpecificationAttributeSearchModel);

            model.ProductSegmentId = productSegmentId;
            model.SpecificationAttributeId = productSpecificationId;

            return View(ProductManagementDefaults.AdminViewPath + "SpecificationAttribute/CreateOrUpdateSpecificationAttributes.cshtml", model);
        }

        [HttpPost]
        public virtual IActionResult ProductSpecAttrList(Models.ProductSpecificationAttributeModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            var vendorId = _workContext.CurrentVendor != null ? _workContext.CurrentVendor.Id : 0;
            int totalRecord = 0;
            var products = _productFilterOptionService.GetProductsBySegmentId(searchModel.ProductSegmentId, out totalRecord, vendorId: vendorId);

            //Prepare specification list
            var model = _segmentModelFactory.PrepareProductSpecificationAttributeListModel(searchModel, products);
            return Json(model);
        }

        public virtual IActionResult ProductSpecAttributeAddOrEdit(int specificationAttributeId, int productSegmentId, int pDD_ProductAttributeMapId, int? id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();


            var model = new ProductSegmentAddSpecificationAttributeModel();

            //prepare specification attribute model to add to the product
            PrepareAddSpecificationAttributeToProductModel(model, specificationAttributeId,id);

            model.ProductSegmentId = productSegmentId;

            return View(ProductManagementDefaults.AdminViewPath + "SpecificationAttribute/ProductSpecAttributeAddOrEdit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult ProductSpecificationAttributeAdd(ProductSegmentAddSpecificationAttributeModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            var vendorId = _workContext.CurrentVendor != null ? _workContext.CurrentVendor.Id : 0;
            int totalRecord = 0;
            var products = _productFilterOptionService.GetProductsBySegmentId(model.ProductSegmentId, out totalRecord, vendorId: vendorId);

            //we allow filtering only for "Option" attribute type
            if (model.AttributeTypeId != (int)SpecificationAttributeType.Option)
                model.AllowFiltering = false;

            //we don't allow CustomValue for "Option" attribute type
            if (model.AttributeTypeId == (int)SpecificationAttributeType.Option)
                model.ValueRaw = null;

            //store raw html if field allow this
            if (model.AttributeTypeId == (int)SpecificationAttributeType.CustomText || model.AttributeTypeId == (int)SpecificationAttributeType.Hyperlink)
                model.ValueRaw = model.Value;


            string specificationValuesId = "";
            for (int i = 0; i < products.Count; i++)
            {
                var psa = new ProductSpecificationAttribute
                {
                    AttributeTypeId = model.AttributeTypeId,
                    SpecificationAttributeOptionId = model.SpecificationAttributeOptionId,
                    ProductId = products[i].Id,
                    CustomValue = model.ValueRaw,
                    AllowFiltering = model.AllowFiltering,
                    ShowOnProductPage = model.ShowOnProductPage,
                    DisplayOrder = model.DisplayOrder
                };
                _specificationAttributeService.InsertProductSpecificationAttribute(psa);

                specificationValuesId = specificationValuesId + psa.Id + ",";
            }

            if (!string.IsNullOrEmpty(specificationValuesId))
                specificationValuesId = specificationValuesId.Substring(0, specificationValuesId.Length - 1);

            //Insert attribute mapping with segment
            var mapper = new PDD_ProductAttributeMap()
            {
                EntityType = EntityTypeEnum.ProductSpecificationMapValue.ToString(),
                EntityId = model.SpecificationAttributeId,
                SegmentId = model.ProductSegmentId,
                AttributeMapperId = specificationValuesId
            };
            _productFilterOptionService.InsertProductAttributeMapWithSegment(mapper);

            if (continueEditing)
                return RedirectToAction("ProductSpecAttributeAddOrEdit", new
                {
                    specificationAttributeId = model.SpecificationAttributeId,
                    productSegmentId = model.ProductSegmentId,
                    pDD_ProductAttributeMapId = model.PDD_ProductAttributeMapId
                });

            return RedirectToAction("AddSpecificationAttributeToProduct", new
            {
                productSpecificationId = model.SpecificationAttributeId,
                productSegmentId = model.ProductSegmentId
            });

        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult ProductSpecAttrUpdate(ProductSegmentAddSpecificationAttributeModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            var vendorId = _workContext.CurrentVendor != null ? _workContext.CurrentVendor.Id : 0;
            int totalRecord = 0;
            var products = _productFilterOptionService.GetProductsBySegmentId(model.ProductSegmentId, out totalRecord, vendorId: vendorId);

            string specificationValuesId = "";
            if (model.PDD_ProductAttributeMapId > 0)
            {
                for (int i = 0; i < products.Count; i++)
                {
                    var isMapWithProduct = _specificationAttributeService.GetProductSpecificationAttributeCount(productId: products[i].Id);
                    if (isMapWithProduct == 0)
                    {
                        string customValue = "";
                        //we allow filtering only for "Option" attribute type
                        if (model.AttributeTypeId != (int)SpecificationAttributeType.Option)
                            model.AllowFiltering = false;

                        //we don't allow CustomValue for "Option" attribute type
                        if (model.AttributeTypeId == (int)SpecificationAttributeType.Option)
                            customValue = null;

                        var psa = new ProductSpecificationAttribute
                        {
                            AttributeTypeId = model.AttributeTypeId,
                            SpecificationAttributeOptionId = model.SpecificationAttributeOptionId,
                            ProductId = products[i].Id,
                            CustomValue = customValue,
                            AllowFiltering = model.AllowFiltering,
                            ShowOnProductPage = model.ShowOnProductPage,
                            DisplayOrder = model.DisplayOrder
                        };
                        _specificationAttributeService.InsertProductSpecificationAttribute(psa);

                        specificationValuesId = specificationValuesId + psa.Id + ",";
                    }
                }

                var segmentMap = _productFilterOptionService.GetProductAttributeMapWithSegmentById(model.PDD_ProductAttributeMapId);

                for (int i = 0; i < segmentMap.AttributeMapperIdList.Count; i++)
                {

                    //try to get a product specification attribute with the specified id
                    var psa = _specificationAttributeService.GetProductSpecificationAttributeById(segmentMap.AttributeMapperIdList[i]);
                    if (psa == null)
                        return Content("No product specification attribute found with the specified id");

                    var productId = psa.Product.Id;

                    //we allow filtering and change option only for "Option" attribute type
                    if (model.AttributeTypeId == (int)SpecificationAttributeType.Option)
                    {
                        psa.AllowFiltering = model.AllowFiltering;
                        psa.SpecificationAttributeOptionId = model.SpecificationAttributeOptionId;
                    }

                    psa.ShowOnProductPage = model.ShowOnProductPage;
                    psa.DisplayOrder = model.DisplayOrder;
                    _specificationAttributeService.UpdateProductSpecificationAttribute(psa);

                    specificationValuesId = specificationValuesId + psa.Id + ",";
                }
                if (!string.IsNullOrEmpty(specificationValuesId))
                    specificationValuesId = specificationValuesId.Substring(0, specificationValuesId.Length - 1);


                segmentMap.AttributeMapperId = specificationValuesId;
                _productFilterOptionService.UpdateProductAttributeMapWithSegment(segmentMap);
            }
            else
            {
                var psa = _specificationAttributeService.GetProductSpecificationAttributeById(model.SpecificationId);

                for (int i = 0; i < products.Count; i++)
                {
                    string customValue = "";
                    //we allow filtering only for "Option" attribute type
                    if (model.AttributeTypeId != (int)SpecificationAttributeType.Option)
                        model.AllowFiltering = false;

                    //we don't allow CustomValue for "Option" attribute type
                    if (model.AttributeTypeId == (int)SpecificationAttributeType.Option)
                        customValue = null;

                    if (psa != null && psa.ProductId == products[i].Id)
                    {
                        //we allow filtering and change option only for "Option" attribute type
                        if (model.AttributeTypeId == (int)SpecificationAttributeType.Option)
                        {
                            psa.AllowFiltering = model.AllowFiltering;
                            psa.SpecificationAttributeOptionId = model.SpecificationAttributeOptionId;
                        }

                        psa.ShowOnProductPage = model.ShowOnProductPage;
                        psa.DisplayOrder = model.DisplayOrder;
                        _specificationAttributeService.UpdateProductSpecificationAttribute(psa);

                        specificationValuesId = specificationValuesId + psa.Id + ",";
                    }
                    else
                    {
                        var psanew = new ProductSpecificationAttribute
                        {
                            AttributeTypeId = model.AttributeTypeId,
                            SpecificationAttributeOptionId = model.SpecificationAttributeOptionId,
                            ProductId = products[i].Id,
                            CustomValue = customValue,
                            AllowFiltering = model.AllowFiltering,
                            ShowOnProductPage = model.ShowOnProductPage,
                            DisplayOrder = model.DisplayOrder
                        };
                        _specificationAttributeService.InsertProductSpecificationAttribute(psanew);
                        specificationValuesId = specificationValuesId + psanew.Id + ",";
                    }
                }

                if (!string.IsNullOrEmpty(specificationValuesId))
                    specificationValuesId = specificationValuesId.Substring(0, specificationValuesId.Length - 1);

                //Insert attribute mapping with segment
                var mapper = new PDD_ProductAttributeMap()
                {
                    EntityType = EntityTypeEnum.ProductSpecificationMapValue.ToString(),
                    EntityId = model.SpecificationAttributeId,
                    SegmentId = model.ProductSegmentId,
                    AttributeMapperId = specificationValuesId
                };
                _productFilterOptionService.InsertProductAttributeMapWithSegment(mapper);

            }

            if (continueEditing)
                return RedirectToAction("ProductSpecAttributeAddOrEdit", new
                {
                    specificationAttributeId = model.SpecificationAttributeId,
                    productSegmentId = model.ProductSegmentId,
                    pDD_ProductAttributeMapId = model.PDD_ProductAttributeMapId
                });

            return RedirectToAction("AddSpecificationAttributeToProduct", new
            {
                productSpecificationId = model.SpecificationAttributeId,
                productSegmentId = model.ProductSegmentId
            });
        }

        [HttpPost]
        public virtual IActionResult ProductSpecAttrDelete(ProductSegmentAddSpecificationAttributeModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            if (model.PDD_ProductAttributeMapId > 0)
            {
                var segmentMap = _productFilterOptionService.GetProductAttributeMapWithSegmentById(model.PDD_ProductAttributeMapId);

                for (int i = 0; i < segmentMap.AttributeMapperIdList.Count; i++)
                {
                    //try to get a product specification attribute with the specified id
                    var psa = _specificationAttributeService.GetProductSpecificationAttributeById(segmentMap.AttributeMapperIdList[i]);
                    if (psa != null)
                        _specificationAttributeService.DeleteProductSpecificationAttribute(psa);
                }
            }
            else
            {
                //try to get a product specification attribute with the specified id
                var psa = _specificationAttributeService.GetProductSpecificationAttributeById(model.Id);
                if (psa != null)
                    _specificationAttributeService.DeleteProductSpecificationAttribute(psa);
            }

            //select an appropriate panel
            SaveSelectedPanelName("segment-SegmentProductSpecAttribute");
            return RedirectToAction("AddSpecificationAttributeToProduct", new
            {
                productSpecificationId = model.SpecificationAttributeId,
                productSegmentId = model.ProductSegmentId
            });
        }

        #endregion

        #endregion
    }
}

