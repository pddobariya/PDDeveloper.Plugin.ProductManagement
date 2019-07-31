using PDDeveloper.Plugin.ProductManagement.Domain;
using Nop.Core;
using Nop.Core.Data;
using Nop.Services.Events;
using System;
using System.Linq;

namespace PDDeveloper.Plugin.ProductManagement.Services
{
    public class ProductSegmentService : IProductSegmentService
    {
        #region Fields

        private readonly IRepository<PDD_ProductSegment> _productSegmentRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor
        public ProductSegmentService(IRepository<PDD_ProductSegment> productSegmentRepository,
            IEventPublisher eventPublisher)
        {
            this._productSegmentRepository = productSegmentRepository;
            this._eventPublisher = eventPublisher;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets all product segment
        /// </summary>
        /// <param name="storeId">The store identifier; pass 0 to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="name">Segment name</param>
        /// <returns>Pickup points</returns>
        public virtual IPagedList<PDD_ProductSegment> GetAllProductSegment(string name, int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _productSegmentRepository.Table;
            if (storeId > 0)
                query = query.Where(segment => segment.StoreId == storeId || segment.StoreId == 0);

            if (name != null && name.Length > 0)
                query = query.Where(segment => segment.Name.Contains(name));

            query = query.OrderBy(segment => segment.DisplayOrder).ThenBy(segment => segment.Name);

            return new PagedList<PDD_ProductSegment>(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Inserts ProductSegment
        /// </summary>
        /// <param name="productSegment">ProductSegment</param>
        public virtual void InsertProductSegment(PDD_ProductSegment productSegment)
        {
            if (productSegment == null)
                throw new ArgumentNullException(nameof(productSegment));

            _productSegmentRepository.Insert(productSegment);
            
            //event notification
            _eventPublisher.EntityInserted(productSegment);
        }

        /// <summary>
        /// Updates the productSegment
        /// </summary>
        /// <param name="productSegment">productSegment</param>
        public virtual void UpdateProductSegment(PDD_ProductSegment productSegment)
        {
            if (productSegment == null)
                throw new ArgumentNullException(nameof(productSegment));

            _productSegmentRepository.Update(productSegment);
            
            //event notification
            _eventPublisher.EntityUpdated(productSegment);
        }

        /// <summary>
        /// Deletes a productSegment
        /// </summary>
        /// <param name="productSegment">productSegment</param>
        public virtual void DeleteProductSegment(PDD_ProductSegment productSegment)
        {
            if (productSegment == null)
                throw new ArgumentNullException(nameof(productSegment));

            _productSegmentRepository.Delete(productSegment);
            
            //event notification
            _eventPublisher.EntityDeleted(productSegment);
        }

        /// <summary>
        /// Gets a product segment
        /// </summary>
        /// <param name="categoryId">Category identifier</param>
        /// <returns>Category</returns>
        public virtual PDD_ProductSegment GetProductSegmentById(int id)
        {
            if (id == 0)
                return null;

            return _productSegmentRepository.GetById(id);
        }
        #endregion
    }
}
