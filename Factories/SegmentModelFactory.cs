using GBS.Plugin.ProductManagement.Domain;
using GBS.Plugin.ProductManagement.Models;
using GBS.Plugin.ProductManagement.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Factories;
using System;
using System.Linq;

namespace GBS.Plugin.ProductManagement.Factories
{
    public class SegmentModelFactory : ISegmentModelFactory
    {
        #region Fields
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IProductSegmentService _productSegmentService;
        private readonly IStoreService _storeService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        #endregion

        #region Ctor
        public SegmentModelFactory(IBaseAdminModelFactory baseAdminModelFactory,
            IProductSegmentService productSegmentService,
            IStoreService storeService,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory)
        {
            this._baseAdminModelFactory = baseAdminModelFactory;
            this._productSegmentService = productSegmentService;
            this._storeService = storeService;
            this._localizationService = localizationService;
            this._localizedModelFactory = localizedModelFactory;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prepare segment search model
        /// </summary>
        /// <param name="searchModel">Segment search model</param>
        /// <returns>Segment search model</returns>
        public virtual ProductSegmentSearchModel PrepareSegmentSearchModel(ProductSegmentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available stores
            _baseAdminModelFactory.PrepareStores(searchModel.AvailableStores);

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged prodcut segment list model
        /// </summary>
        /// <param name="searchModel">Product segment search model</param>
        /// <returns>Product segment list model</returns>
        public virtual ProductSegmentListModel PrepareProductSegmentListModel(ProductSegmentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get product segment
            var segments = _productSegmentService.GetAllProductSegment(name : searchModel.SearchSegmentName,
                storeId: searchModel.SearchStoreId,
                pageIndex: searchModel.Page - 1, 
                pageSize: searchModel.PageSize);

            //prepare grid model
            var segmetnEntity = segments.Select(productSegment =>
            {
                var store = _storeService.GetStoreById(productSegment.StoreId);
                return new ProductSegmentModel
                {
                    Id = productSegment.Id,
                    Name = productSegment.Name,
                    Description = productSegment.Description,
                    DisplayOrder = productSegment.DisplayOrder,
                    StoreId = productSegment.StoreId,
                    StoreName = store?.Name ?? (productSegment.StoreId == 0 ? _localizationService.GetResource("Admin.Configuration.Settings.StoreScope.AllStores") : string.Empty)
                };
            }).ToList();

            return new ProductSegmentListModel
            {
                Data = segmetnEntity,
                Total = segments.TotalCount
            };
        }

        /// <summary>
        /// Prepare product segment model
        /// </summary>
        /// <param name="model">Product segment model</param>
        /// <param name="productSegment">productSegment</param>
        /// <returns>ProductSegmentModel</returns>
        public virtual ProductSegmentModel PrepareProductSegmentModel(ProductSegmentModel model, GBS_ProductSegment productSegment)
        {
            if (productSegment != null)
            {
                //fill in model values from the entity
                var store = _storeService.GetStoreById(productSegment.StoreId);
                model =  new ProductSegmentModel
                {
                    Id = productSegment.Id,
                    Name = productSegment.Name,
                    Description = productSegment.Description,
                    DisplayOrder = productSegment.DisplayOrder,
                    StoreId = productSegment.StoreId,
                    StoreName = store?.Name ?? (productSegment.StoreId == 0 ? _localizationService.GetResource("Admin.Configuration.Settings.StoreScope.AllStores") : string.Empty)
                };
            }
            
            //prepare storelist
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Configuration.Settings.StoreScope.AllStores"), Value = "0" });
            foreach (var store in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = store.Name, Value = store.Id.ToString() });

            return model;
        }
        #endregion
    }
}
