using GBS.Plugin.ProductManagement.Domain;
using GBS.Plugin.ProductManagement.Factories;
using GBS.Plugin.ProductManagement.Models;
using GBS.Plugin.ProductManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using System;

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
        #endregion

        #region Ctor
        public ProductSegmentController(IPermissionService permissionService,
            ISegmentModelFactory segmentModelFactory,
            IProductSegmentService productSegmentService,
            ILocalizationService localizationService,
            ICustomerActivityService customerActivityService)
        {
            this._permissionService = permissionService;
            this._segmentModelFactory = segmentModelFactory;
            this._productSegmentService = productSegmentService;
            this._localizationService = localizationService;
            this._customerActivityService = customerActivityService;
        }
        #endregion
        
        #region Methods

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
    }
}
