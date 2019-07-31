using Autofac;
using Autofac.Core;
using PDDeveloper.Plugin.ProductManagement.Data;
using PDDeveloper.Plugin.ProductManagement.Domain;
using PDDeveloper.Plugin.ProductManagement.Factories;
using PDDeveloper.Plugin.ProductManagement.Services;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Web.Framework.Infrastructure.Extensions;

namespace PDDeveloper.Plugin.ProductManagement.Infrastructure
{
    /// <summary>
    /// Dependency registrar
    /// </summary>
    public class DependencyRegistrar : IDependencyRegistrar
    {
        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="builder">Container builder</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="config">Config</param>
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            //Factory
            builder.RegisterType<SegmentModelFactory>().As<ISegmentModelFactory>().InstancePerLifetimeScope();

            //Services
            builder.RegisterType<ProductSegmentService>().As<IProductSegmentService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductFilterOptionService>().As<IProductFilterOptionService>().InstancePerLifetimeScope();

            //data context
            builder.RegisterPluginDataContext<ProductManagementObjectContext>("nop_object_context_product_segment");

            //override required repository with our custom context
            builder.RegisterType<EfRepository<PDD_ProductSegment>>().As<IRepository<PDD_ProductSegment>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_product_segment"))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<PDD_ProductFilterOptions>>().As<IRepository<PDD_ProductFilterOptions>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_product_segment"))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<PDD_Product_Include_Exclude>>().As<IRepository<PDD_Product_Include_Exclude>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_product_segment"))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<PDD_ProductAttributeMap>>().As<IRepository<PDD_ProductAttributeMap>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_product_segment"))
                .InstancePerLifetimeScope();
        }

        /// <summary>
        /// Order of this dependency registrar implementation
        /// </summary>
        public int Order => 1;
    }
}