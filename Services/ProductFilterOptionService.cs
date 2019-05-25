using GBS.Plugin.ProductManagement.Domain;
using GBS.Plugin.ProductManagement.Domain.Enums;
using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GBS.Plugin.ProductManagement.Services
{
    public class ProductFilterOptionService : IProductFilterOptionService
    {
        #region Fields

        private readonly IRepository<ProductFilterOptions> _productFilterOptionRepository;
        private readonly IRepository<Product_Include_Exclude> _product_Include_ExcludeRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor
        public ProductFilterOptionService(IRepository<ProductFilterOptions> productFilterOptionRepository,
            IEventPublisher eventPublisher,
            IRepository<Product_Include_Exclude> product_Include_ExcludeRepository)
        {
            this._productFilterOptionRepository = productFilterOptionRepository;
            this._eventPublisher = eventPublisher;
            this._product_Include_ExcludeRepository = product_Include_ExcludeRepository;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Get all Filter opction
        /// </summary>
        /// <param name="segmentId">Segment Identity</param>
        /// <returns></returns>
        public IList<ProductFilterOptions> GetAllFilterOptionBySegmentId(int segmentId = 0)
        {
            if (segmentId == 0)
                return new List<ProductFilterOptions>();

            return _productFilterOptionRepository.Table.Where(p => p.ProductSegmentManagerId == segmentId).OrderByDescending(p => p.Id).ToList();
        }

        /// <summary>
        /// Insert product segment opction
        /// </summary>
        /// <param name="productFilterOption"></param>
        public void InsertProductFilterOption(ProductFilterOptions productFilterOption)
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
        public void UpdateProductFilterOption(ProductFilterOptions productFilterOption)
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
        public void DeleteProductFilterOption(ProductFilterOptions productFilterOption)
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
        public ProductFilterOptions GetProductFilterOptionById(int id)
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
        public IList<Product_Include_Exclude> GetAllIncludeExcludeProductBySegmentId(int segmentId, SegmentProductType segmentProductType,int productId =0)
        {
            if (segmentId == 0)
                return new List<Product_Include_Exclude>();

            var productids =  _product_Include_ExcludeRepository.Table.Where(p => p.ProductSegmentManagerId == segmentId && p.ProductType == (int)segmentProductType).OrderByDescending(p => p.Id).ToList();

            if (productId > 0)
                productids = productids.Where(p => p.ProductId == productId).ToList();

            return productids;
        }

        /// <summary>
        /// Insert IncludeExcludeProduct
        /// </summary>
        /// <param name="product_Include_Exclude"></param>
        public void InsertIncludeExcludeProduct(Product_Include_Exclude product_Include_Exclude)
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
        public void UpdateIncludeExcludeProduct(Product_Include_Exclude product_Include_Exclude)
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
        public void DeleteIncludeExcludeProduct(Product_Include_Exclude product_Include_Exclude)
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
        public Product_Include_Exclude GetIncludeExcludeProductById(int id)
        {
            if (id == 0)
                return null;

            return _product_Include_ExcludeRepository.GetById(id);
        }
        #endregion
    }
}
