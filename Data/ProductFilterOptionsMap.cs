using GBS.Plugin.ProductManagement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;

namespace GBS.Plugin.ProductManagement.Data
{
    /// <summary>
    /// Represents a product segment opction(Condition for getting data) mapping cofiguraction
    /// </summary>
    public partial class ProductFilterOptionsMap : NopEntityTypeConfiguration<GBS_ProductFilterOptions>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<GBS_ProductFilterOptions> builder)
        {
            builder.ToTable(nameof(GBS_ProductFilterOptions));
            builder.HasKey(segmet => segmet.Id);
        }

        #endregion
    }
}
