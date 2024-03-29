﻿using PDDeveloper.Plugin.ProductManagement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;

namespace PDDeveloper.Plugin.ProductManagement.Data
{
    /// <summary>
    /// Represents a product segment mapping cofiguraction
    /// </summary>
    public partial class ProductSegmentMap : NopEntityTypeConfiguration<PDD_ProductSegment>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<PDD_ProductSegment> builder)
        {
            builder.ToTable(nameof(PDD_ProductSegment));
            builder.HasKey(segmet => segmet.Id);

            builder.Property(segmet => segmet.Name).IsRequired();
            builder.Property(segmet => segmet.DisplayOrder).HasColumnType("int");
        }

        #endregion
    }
}
