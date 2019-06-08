using GBS.Plugin.ProductManagement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;

namespace GBS.Plugin.ProductManagement.Data
{
    /// <summary>
    /// Represents a attribute mapping with segment
    /// </summary>
    public class GBS_ProductAttributeMap_Map : NopEntityTypeConfiguration<GBS_ProductAttributeMap>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<GBS_ProductAttributeMap> builder)
        {
            builder.ToTable(nameof(GBS_ProductAttributeMap));
            builder.HasKey(segmet => segmet.Id);
        }

        #endregion
    }
}
