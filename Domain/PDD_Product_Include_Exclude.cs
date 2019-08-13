using Nop.Core;

namespace PDDeveloper.Plugin.ProductManagement.Domain
{
    /// <summary>
    /// Represents a product segment opction(Condition for getting data)
    /// </summary>
    public class PDD_Product_Include_Exclude : BaseEntity
    {
        /// <summary>
        /// Get or set ProductSegmentManager Identity
        /// </summary>
        public int ProductSegmentManagerId { get; set; }

        /// <summary>
        /// add type for product Include to exclude from enum
        /// </summary>
        public int ProductType { get; set; }

        /// <summary>
        /// Product identity
        /// </summary>
        public int ProductId { get; set; }
    }
}
