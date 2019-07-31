using Nop.Core;
using System;

namespace PDDeveloper.Plugin.ProductManagement.Domain
{
    /// <summary>
    /// Represents a segment of product
    /// </summary>
    public partial class PDD_ProductSegment : BaseEntity
    {
        /// <summary>
        /// Gets or sets a name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets a store identifier
        /// </summary>
        public int StoreId { get; set; }
        
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
