using Nop.Core;
using System;

namespace GBS.Plugin.ProductManagement.Domain
{
    /// <summary>
    /// Represents a product segment opction(Condition for getting data)
    /// </summary>
    public class GBS_ProductFilterOptions : BaseEntity
    {
        /// <summary>
        /// Get or set ProductSegmentManager Identity
        /// </summary>
        public int ProductSegmentManagerId { get; set; }

        /// <summary>
        /// Get or set BeginsWith
        /// </summary>
        public string BeginsWith { get; set; }

        /// <summary>
        /// Get or set EndsWith
        /// </summary>
        public string EndsWith { get; set; }

        /// <summary>
        /// Get or set DoesNotEndWith
        /// </summary>
        public string DoesNotEndWith { get; set; }

        /// <summary>
        /// Get or set Contains
        /// </summary>
        public string Contains { get; set; }

        /// <summary>
        /// Get or set created on
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Get or set UpdateOnUTC
        /// </summary>
        public DateTime UpdatedOnUtc { get; set; }
    }
}
