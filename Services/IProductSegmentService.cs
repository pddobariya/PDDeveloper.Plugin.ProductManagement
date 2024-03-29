﻿using PDDeveloper.Plugin.ProductManagement.Domain;
using Nop.Core;

namespace PDDeveloper.Plugin.ProductManagement.Services
{
    public interface IProductSegmentService
    {
        /// <summary>
        /// Gets all segments
        /// </summary>
        /// <param name="storeId">The store identifier; pass 0 to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="name">segment name</param>
        /// <returns>Product segmetns</returns>
        IPagedList<PDD_ProductSegment> GetAllProductSegment(string name, int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Inserts product segment
        /// </summary>
        /// <param name="productSegment">ProductSegment</param>
        void InsertProductSegment(PDD_ProductSegment productSegment);

        /// <summary>
        /// Updates the productSegment
        /// </summary>
        /// <param name="productSegment">productSegment</param>
        void UpdateProductSegment(PDD_ProductSegment productSegment);

        /// <summary>
        /// Deletes a productSegment
        /// </summary>
        /// <param name="productSegment">productSegment</param>
        void DeleteProductSegment(PDD_ProductSegment productSegment);

        /// <summary>
        /// Get product segment by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Product Segment</returns>
        PDD_ProductSegment GetProductSegmentById(int id);
    }
}
