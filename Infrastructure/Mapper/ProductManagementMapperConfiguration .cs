using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using Nop.Web.Framework.Models;
using PDDeveloper.Plugin.ProductManagement.Domain;
using PDDeveloper.Plugin.ProductManagement.Models;

namespace PDDeveloper.Plugin.ProductManagement.Infrastructure.Mapper
{
    /// <summary>
    /// AutoMapper configuration for admin area models
    /// </summary>
    public class ProductManagementMapperConfiguration : Profile, IOrderedMapperProfile
    {
        #region Ctor
        public ProductManagementMapperConfiguration()
        {
            //create specific maps
            CreateProductSegmentOpctionMaps();

            //add some generic mapping rules
            ForAllMaps((mapConfiguration, map) =>
            {
                //exclude Form and CustomProperties from mapping BaseNopModel
                if (typeof(BaseNopModel).IsAssignableFrom(mapConfiguration.DestinationType))
                {
                    //map.ForMember(nameof(BaseNopModel.Form), options => options.Ignore());
                    map.ForMember(nameof(BaseNopModel.CustomProperties), options => options.Ignore());
                }
            });
        }
        #endregion
        #region Utilities
        /// <summary>
        /// Create product segment opction map
        /// </summary>
        protected virtual void CreateProductSegmentOpctionMaps()
        {
            CreateMap<PDD_ProductFilterOptions, ProductFilterOptionsModel>()
               .ForMember(model => model.CustomProperties, options => options.Ignore());

            CreateMap<ProductFilterOptionsModel, PDD_ProductFilterOptions>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.ProductSegmentManagerId, options => options.Ignore());
        }
        #endregion

        #region Properties

        /// <summary>
        /// Order of this mapper implementation
        /// </summary>
        public int Order => -1;

        #endregion
    }
}
