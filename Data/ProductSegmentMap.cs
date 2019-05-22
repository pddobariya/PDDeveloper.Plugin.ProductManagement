﻿using GBS.Plugin.ProductManagement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;

namespace GBS.Plugin.ProductManagement.Data
{
    /// <summary>
    /// Represents a product segment mapping cofiguraction
    /// </summary>
    public partial class ProductSegmentMap : NopEntityTypeConfiguration<ProductSegment>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<ProductSegment> builder)
        {
            builder.ToTable(nameof(ProductSegment));
            builder.HasKey(segmet => segmet.Id);

            builder.Property(segmet => segmet.Name).IsRequired();
            builder.Property(segmet => segmet.DisplayOrder).HasColumnType("int");
        }

        #endregion
    }
}