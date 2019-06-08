using Nop.Core;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace GBS.Plugin.ProductManagement.Domain
{
    /// <summary>
    /// Represents a product attribute Map with segment
    /// </summary>
    public class GBS_ProductAttributeMap : BaseEntity
    {
        /// <summary>
        /// Get or set  segment identity
        /// </summary>
        public int SegmentId { get; set; }

        /// <summary>
        /// Get or set comma seprated attributemapper Identity
        /// </summary>
        public string AttributeMapperId { get; set; }

        /// <summary>
        /// Get or set Entity Identity
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Get or set Entity Type
        /// </summary>
        public string EntityType { get; set; }

        [NotMapped]
        public List<int> AttributeMapperIdList { get { return AttributeMapperId.Split(',').Select(int.Parse).ToList(); } }
    }
}
