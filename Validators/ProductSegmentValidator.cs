using FluentValidation;
using PDDeveloper.Plugin.ProductManagement.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace PDDeveloper.Plugin.ProductManagement.Validators
{
    public partial class ProductSegmentValidator : BaseNopValidator<ProductSegmentModel>
    {
        public ProductSegmentValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Plugins.GBS.ProductManagement.Segment.Fields.Name.Required"));
            
        }
    }
}