﻿using PDDeveloper.Plugin.ProductManagement.Data;
using PDDeveloper.Plugin.ProductManagement.Domain;
using PDDeveloper.Plugin.ProductManagement.Domain.Enums;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Data.Extensions;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PDDeveloper.Plugin.ProductManagement.Services
{
    public class ProductFilterOptionService : IProductFilterOptionService
    {
        #region Fields

        private readonly IRepository<PDD_ProductFilterOptions> _productFilterOptionRepository;
        private readonly IRepository<PDD_Product_Include_Exclude> _product_Include_ExcludeRepository;
        private readonly IRepository<PDD_ProductAttributeMap> _productAttributeMapRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly IDataProvider _dataProvider;
        private readonly ProductManagementObjectContext _dbContext;
        private readonly NopObjectContext _nopObjectContext;

        #endregion

        #region Ctor
        public ProductFilterOptionService(IRepository<PDD_ProductFilterOptions> productFilterOptionRepository,
            IRepository<PDD_Product_Include_Exclude> product_Include_ExcludeRepository,
            IRepository<PDD_ProductAttributeMap> productAttributeMapRepository,
            IEventPublisher eventPublisher,
            IDataProvider dataProvider,
            ProductManagementObjectContext dbContext,
            NopObjectContext nopObjectContext)
        {
            this._productFilterOptionRepository = productFilterOptionRepository;
            this._product_Include_ExcludeRepository = product_Include_ExcludeRepository;
            this._productAttributeMapRepository = productAttributeMapRepository;
            this._eventPublisher = eventPublisher;
            this._dataProvider = dataProvider;
            this._dbContext = dbContext;
            this._nopObjectContext = nopObjectContext;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Get all Filter opction
        /// </summary>
        /// <param name="segmentId">Segment Identity</param>
        /// <returns></returns>
        public IList<PDD_ProductFilterOptions> GetAllFilterOptionBySegmentId(int segmentId = 0)
        {
            if (segmentId == 0)
                return new List<PDD_ProductFilterOptions>();

            return _productFilterOptionRepository.Table.Where(p => p.ProductSegmentManagerId == segmentId).OrderByDescending(p => p.Id).ToList();
        }

        /// <summary>
        /// Insert product segment opction
        /// </summary>
        /// <param name="productFilterOption"></param>
        public void InsertProductFilterOption(PDD_ProductFilterOptions productFilterOption)
        {
            if (productFilterOption == null)
                throw new ArgumentNullException(nameof(productFilterOption));

            _productFilterOptionRepository.Insert(productFilterOption);

            //event notification
            _eventPublisher.EntityInserted(productFilterOption);
        }

        /// <summary>
        /// Updte product segment opction
        /// </summary>
        /// <param name="productFilterOption"></param>
        public void UpdateProductFilterOption(PDD_ProductFilterOptions productFilterOption)
        {
            if (productFilterOption == null)
                throw new ArgumentNullException(nameof(productFilterOption));

            _productFilterOptionRepository.Update(productFilterOption);

            //event notification
            _eventPublisher.EntityUpdated(productFilterOption);
        }

        /// <summary>
        /// Delete product segment opction
        /// </summary>
        /// <param name="productFilterOption"></param>
        public void DeleteProductFilterOption(PDD_ProductFilterOptions productFilterOption)
        {
            if (productFilterOption == null)
                throw new ArgumentNullException(nameof(productFilterOption));

            _productFilterOptionRepository.Delete(productFilterOption);

            //event notification
            _eventPublisher.EntityDeleted(productFilterOption);
        }

        /// <summary>
        /// get product segment opction
        /// </summary>
        /// <param name="id">Opction identity</param>
        /// <returns>product segment opction</returns>
        public PDD_ProductFilterOptions GetProductFilterOptionById(int id)
        {
            if (id == 0)
                return null;

            return _productFilterOptionRepository.GetById(id);
        }


        #endregion

        #region Method for Include & Exclude
        /// <summary>
        /// Get all Filter opction
        /// </summary>
        /// <param name="segmentId">Segment Identity</param>
        /// <returns></returns>
        public IList<PDD_Product_Include_Exclude> GetAllIncludeExcludeProductBySegmentId(int segmentId, SegmentProductType segmentProductType,int productId =0)
        {
            if (segmentId == 0)
                return new List<PDD_Product_Include_Exclude>();

            var productids =  _product_Include_ExcludeRepository.Table.Where(p => p.ProductSegmentManagerId == segmentId && p.ProductType == (int)segmentProductType).OrderByDescending(p => p.Id).ToList();

            if (productId > 0)
                productids = productids.Where(p => p.ProductId == productId).ToList();

            return productids;
        }

        /// <summary>
        /// Insert IncludeExcludeProduct
        /// </summary>
        /// <param name="product_Include_Exclude"></param>
        public void InsertIncludeExcludeProduct(PDD_Product_Include_Exclude product_Include_Exclude)
        {
            if (product_Include_Exclude == null)
                throw new ArgumentNullException(nameof(product_Include_Exclude));

            _product_Include_ExcludeRepository.Insert(product_Include_Exclude);

            //event notification
            _eventPublisher.EntityInserted(product_Include_Exclude);
        }

        /// <summary>
        /// Updte IncludeExcludeProduct
        /// </summary>
        /// <param name="product_Include_Exclude"></param>
        public void UpdateIncludeExcludeProduct(PDD_Product_Include_Exclude product_Include_Exclude)
        {
            if (product_Include_Exclude == null)
                throw new ArgumentNullException(nameof(product_Include_Exclude));

            _product_Include_ExcludeRepository.Update(product_Include_Exclude);

            //event notification
            _eventPublisher.EntityUpdated(product_Include_Exclude);
        }

        /// <summary>
        /// Delete IncludeExcludeProduct
        /// </summary>
        /// <param name="product_Include_Exclude"></param>
        public void DeleteIncludeExcludeProduct(PDD_Product_Include_Exclude product_Include_Exclude)
        {
            if (product_Include_Exclude == null)
                throw new ArgumentNullException(nameof(product_Include_Exclude));

            _product_Include_ExcludeRepository.Delete(product_Include_Exclude);

            //event notification
            _eventPublisher.EntityDeleted(product_Include_Exclude);
        }

        /// <summary>
        /// get IncludeExcludeProduct
        /// </summary>
        /// <param name="id">Opction identity</param>
        /// <returns>Product_Include_Exclude</returns>
        public PDD_Product_Include_Exclude GetIncludeExcludeProductById(int id)
        {
            if (id == 0)
                return null;

            return _product_Include_ExcludeRepository.GetById(id);
        }
        #endregion

        #region Match product
        /// <summary>
        /// Get product list by segment id
        /// </summary>
        /// <param name="productSegmentManagerId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IList<Product> GetProductsBySegmentId(int productSegmentManagerId, out int totalRecords, int pageIndex = 0,int pageSize  = int.MaxValue,int vendorId = 0)
        {
            //some databases don't support int.MaxValue
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            var pProductSegmentManagerId = _dataProvider.GetInt32Parameter("ProductSegmentManagerId", productSegmentManagerId);
            var pPageIndex = _dataProvider.GetInt32Parameter("PageIndex", pageIndex);
            var pPageSize = _dataProvider.GetInt32Parameter("PageSize", pageSize);
            var pVendorId = _dataProvider.GetInt32Parameter("VendorId", vendorId);

            //prepare output parameters
            var totalRecordsParameter = _dataProvider.GetOutputInt32Parameter("TotalRecords");

            var products = _nopObjectContext.EntityFromSql<Product>("PDD_GetProductBySegmentId", pProductSegmentManagerId, pPageIndex, pPageSize, pVendorId, totalRecordsParameter);
            
            totalRecords = totalRecordsParameter.Value != DBNull.Value ? Convert.ToInt32(totalRecordsParameter.Value) : 0;
            if (products != null)
                return products.ToList();
            else
                return null;
        }
        #endregion

        #region Match product attribute
        /// <summary>
        /// Get product attribute list by segment id
        /// </summary>
        /// <param name="productSegmentManagerId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IList<ProductAttribute> GetProductAttributeBySegmentId(int productSegmentManagerId, out int totalRecords, int pageIndex = 0, int pageSize = int.MaxValue,int vendorId =0)
        {
            var pProductSegmentManagerId = _dataProvider.GetInt32Parameter("ProductSegmentManagerId", productSegmentManagerId);
            var pPageIndex = _dataProvider.GetInt32Parameter("PageIndex", pageIndex);
            var pPageSize = _dataProvider.GetInt32Parameter("PageSize", pageSize);
            var pVendorId = _dataProvider.GetInt32Parameter("VendorId", vendorId);

            //prepare output parameters
            var pTotalRecords = _dataProvider.GetOutputInt32Parameter("TotalRecords");
            pTotalRecords.Size = int.MaxValue - 1;

            var productAttributes = _nopObjectContext.EntityFromSql<ProductAttribute>("PDD_GetProductAttributeBySegmentId", pProductSegmentManagerId, pPageIndex, pPageSize, pVendorId, pTotalRecords);

            totalRecords = pTotalRecords.Value != DBNull.Value ? Convert.ToInt32(pTotalRecords.Value) : 0;
            if (productAttributes != null)
                return productAttributes.ToList();
            else
                return null;
        }
        #endregion

        #region Match specification attribute
        /// <summary>
        /// Get specification attribute list by segment id
        /// </summary>
        /// <param name="productSegmentManagerId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IList<SpecificationAttribute> GetProductSpecificationAttributeBySegmentId(int productSegmentManagerId, out int totalRecords, int pageIndex = 0, int pageSize = int.MaxValue,int vendorId = 0)
        {
            var pProductSegmentManagerId = _dataProvider.GetInt32Parameter("ProductSegmentManagerId", productSegmentManagerId);
            var pPageIndex = _dataProvider.GetInt32Parameter("PageIndex", pageIndex);
            var pPageSize = _dataProvider.GetInt32Parameter("PageSize", pageSize);
            var pVendorId = _dataProvider.GetInt32Parameter("VendorId", vendorId);

            //prepare output parameters
            var pTotalRecords = _dataProvider.GetOutputInt32Parameter("TotalRecords");
            pTotalRecords.Size = int.MaxValue - 1;

            var specificationAttributes = _nopObjectContext.EntityFromSql<SpecificationAttribute>("PDD_GetProductSpecificationAttributeBySegmentId", pProductSegmentManagerId, pPageIndex, pPageSize, pVendorId, pTotalRecords);

            totalRecords = pTotalRecords.Value != DBNull.Value ? Convert.ToInt32(pTotalRecords.Value) : 0;
            if (specificationAttributes != null)
                return specificationAttributes.ToList();
            else
                return null;
        }
        #endregion

        #region Product attrbute Mapping
        /// <summary>
        /// Insert attribute mapping with segment
        /// </summary>
        /// <param name="PDD_ProductAttributeMap"></param>
        public void InsertProductAttributeMapWithSegment(PDD_ProductAttributeMap pdd_ProductAttributeMap)
        {
            if (pdd_ProductAttributeMap == null)
                throw new ArgumentNullException(nameof(pdd_ProductAttributeMap));

            _productAttributeMapRepository.Insert(pdd_ProductAttributeMap);

            //event notification
            _eventPublisher.EntityInserted(pdd_ProductAttributeMap);
        }

        /// <summary>
        /// Update PDD_ProductAttributeMap
        /// </summary>
        /// <param name="PDD_ProductAttributeMap"></param>
        public void UpdateProductAttributeMapWithSegment(PDD_ProductAttributeMap pdd_ProductAttributeMap)
        {
            if (pdd_ProductAttributeMap == null)
                throw new ArgumentNullException(nameof(pdd_ProductAttributeMap));

            _productAttributeMapRepository.Update(pdd_ProductAttributeMap);

            //event notification
            _eventPublisher.EntityUpdated(pdd_ProductAttributeMap);
        }

        /// <summary>
        /// Get prduct attribute with segment
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="entityTypeEnum"></param>
        /// <param name="segmentId"></param>
        /// <returns></returns>
        public List<PDD_ProductAttributeMap> GetProductAttributeMapWithSegment(int entityId, EntityTypeEnum entityTypeEnum, int segmentId)
        {
            return _productAttributeMapRepository.Table.Where(p => p.EntityId == entityId && p.EntityType == entityTypeEnum.ToString() && p.SegmentId == segmentId).ToList();
        }

        /// <summary>
        /// Get prduct attribute with segment
        /// </summary>
        /// <param name="attributeMapperId"></param>
        /// <param name="entityId"></param>
        /// <param name="entityTypeEnum"></param>
        /// <param name="segmentId"></param>
        /// <returns></returns>
        public PDD_ProductAttributeMap GetProductAttributeMapWithSegmentByAttributeMapperId(int attributeMapperId,int entityId, EntityTypeEnum entityTypeEnum, int segmentId)
        {
            var productAttributeMap = _productAttributeMapRepository.Table.Where(p => p.EntityId == entityId && p.EntityType == entityTypeEnum.ToString() && p.SegmentId == segmentId);

            return productAttributeMap.Where(p => p.AttributeMapperIdList.Contains(attributeMapperId)).FirstOrDefault();
        }

        /// <summary>
        /// Get prduct attribute with segment
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PDD_ProductAttributeMap GetProductAttributeMapWithSegmentById(int id)
        {
            if (id == 0)
                return null;

            return _productAttributeMapRepository.GetById(id);
        }

        /// <summary>
        /// Delete segment attribute map
        /// </summary>
        /// <param name="PDD_ProductAttributeMap"></param>
        public void DeleteProductAttributeMapWithSegment(PDD_ProductAttributeMap pdd_ProductAttributeMap)
        {
            if (pdd_ProductAttributeMap == null)
                throw new ArgumentNullException(nameof(pdd_ProductAttributeMap));

            _productAttributeMapRepository.Delete(pdd_ProductAttributeMap);

            //event notification
            _eventPublisher.EntityDeleted(pdd_ProductAttributeMap);
        }
        

        #endregion
    }
}
