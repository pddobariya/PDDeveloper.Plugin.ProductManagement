using PDDeveloper.Plugin.ProductManagement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;

namespace PDDeveloper.Plugin.ProductManagement.Data
{
    /// <summary>
    /// Represents a attribute mapping with segment
    /// </summary>
    public class PDD_ProductAttributeMap_Map : NopEntityTypeConfiguration<PDD_ProductAttributeMap>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<PDD_ProductAttributeMap> builder)
        {
            builder.ToTable(nameof(PDD_ProductAttributeMap));
            builder.HasKey(segmet => segmet.Id);
        }

        #endregion
    }
}
