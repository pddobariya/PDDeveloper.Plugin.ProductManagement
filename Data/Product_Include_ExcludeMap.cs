using GBS.Plugin.ProductManagement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;

namespace GBS.Plugin.ProductManagement.Data
{
    public partial class Product_Include_ExcludeMap : NopEntityTypeConfiguration<GBS_Product_Include_Exclude>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<GBS_Product_Include_Exclude> builder)
        {
            builder.ToTable(nameof(GBS_Product_Include_Exclude));
            builder.HasKey(segmet => segmet.Id);
        }

        #endregion
    }
}
