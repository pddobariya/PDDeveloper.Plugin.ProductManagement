using PDDeveloper.Plugin.ProductManagement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;

namespace PDDeveloper.Plugin.ProductManagement.Data
{
    public partial class Product_Include_ExcludeMap : NopEntityTypeConfiguration<PDD_Product_Include_Exclude>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<PDD_Product_Include_Exclude> builder)
        {
            builder.ToTable(nameof(PDD_Product_Include_Exclude));
            builder.HasKey(segmet => segmet.Id);
        }

        #endregion
    }
}
