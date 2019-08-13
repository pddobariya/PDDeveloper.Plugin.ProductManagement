using PDDeveloper.Plugin.ProductManagement.Domain;
using PDDeveloper.Plugin.ProductManagement.Models;
using PDDeveloper.Plugin.ProductManagement.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Nop.Web.Framework.Models.Extensions;

namespace PDDeveloper.Plugin.ProductManagement.Factories
{
    public class SegmentModelFactory : ISegmentModelFactory
    {
        #region Fields
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IProductSegmentService _productSegmentService;
        private readonly IStoreService _storeService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductService _productService;
        private readonly IPictureService _pictureService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IProductFilterOptionService _productFilterOptionService;
        #endregion

        #region Ctor
        public SegmentModelFactory(IBaseAdminModelFactory baseAdminModelFactory,
            IProductSegmentService productSegmentService,
            IStoreService storeService,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            IProductAttributeService productAttributeService,
            IProductAttributeParser productAttributeParser,
            IProductService productService,
            IPictureService pictureService,
            ISpecificationAttributeService specificationAttributeService,
            IProductFilterOptionService productFilterOptionService)
        {
            this._baseAdminModelFactory = baseAdminModelFactory;
            this._productSegmentService = productSegmentService;
            this._storeService = storeService;
            this._localizationService = localizationService;
            this._localizedModelFactory = localizedModelFactory;
            this._productAttributeService = productAttributeService;
            this._productAttributeParser = productAttributeParser;
            this._productService = productService;
            this._pictureService = pictureService;
            this._specificationAttributeService = specificationAttributeService;
            this._productFilterOptionService = productFilterOptionService;
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Prepare product attribute condition model
        /// </summary>
        /// <param name="model">Product attribute condition model</param>
        /// <param name="productAttributeMapping">Product attribute mapping</param>
        protected virtual void PrepareProductAttributeConditionModel(ProductAttributeConditionModel model,
            ProductAttributeMapping productAttributeMapping)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (productAttributeMapping == null)
                throw new ArgumentNullException(nameof(productAttributeMapping));

            model.ProductAttributeMappingId = productAttributeMapping.Id;
            model.EnableCondition = !string.IsNullOrEmpty(productAttributeMapping.ConditionAttributeXml);

            //pre-select attribute and values
            var selectedPva = _productAttributeParser
                .ParseProductAttributeMappings(productAttributeMapping.ConditionAttributeXml)
                .FirstOrDefault();

            var attributes = _productAttributeService.GetProductAttributeMappingsByProductId(productAttributeMapping.ProductId)
                //ignore non-combinable attributes (should have selectable values)
                .Where(x => x.CanBeUsedAsCondition())
                //ignore this attribute (it cannot depend on itself)
                .Where(x => x.Id != productAttributeMapping.Id)
                .ToList();
            foreach (var attribute in attributes)
            {
                var attributeModel = new ProductAttributeConditionModel.ProductAttributeModel
                {
                    Id = attribute.Id,
                    ProductAttributeId = attribute.ProductAttributeId,
                    Name = attribute.ProductAttribute.Name,
                    TextPrompt = attribute.TextPrompt,
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var attributeValueModel = new ProductAttributeConditionModel.ProductAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.Name,
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(attributeValueModel);
                    }

                    //pre-select attribute and value
                    if (selectedPva != null && attribute.Id == selectedPva.Id)
                    {
                        //attribute
                        model.SelectedProductAttributeId = selectedPva.Id;

                        //values
                        switch (attribute.AttributeControlType)
                        {
                            case AttributeControlType.DropdownList:
                            case AttributeControlType.RadioList:
                            case AttributeControlType.Checkboxes:
                            case AttributeControlType.ColorSquares:
                            case AttributeControlType.ImageSquares:
                                {
                                    if (!string.IsNullOrEmpty(productAttributeMapping.ConditionAttributeXml))
                                    {
                                        //clear default selection
                                        foreach (var item in attributeModel.Values)
                                            item.IsPreSelected = false;

                                        //select new values
                                        var selectedValues = _productAttributeParser.ParseProductAttributeValues(productAttributeMapping.ConditionAttributeXml);
                                        foreach (var attributeValue in selectedValues)
                                            foreach (var item in attributeModel.Values)
                                                if (attributeValue.Id == item.Id)
                                                    item.IsPreSelected = true;
                                    }
                                }
                                break;
                            case AttributeControlType.ReadonlyCheckboxes:
                            case AttributeControlType.TextBox:
                            case AttributeControlType.MultilineTextbox:
                            case AttributeControlType.Datepicker:
                            case AttributeControlType.FileUpload:
                            default:
                                //these attribute types are supported as conditions
                                break;
                        }
                    }
                }

                model.ProductAttributes.Add(attributeModel);
            }
        }

        /// <summary>
        /// Prepare product attribute value search model
        /// </summary>
        /// <param name="searchModel">Product attribute value search model</param>
        /// <param name="productAttributeMapping">Product attribute mapping</param>
        /// <returns>Product attribute value search model</returns>
        protected virtual ProductAttributeValueSearchModel PrepareProductAttributeValueSearchModel(ProductAttributeValueSearchModel searchModel,
            ProductAttributeMapping productAttributeMapping)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (productAttributeMapping == null)
                throw new ArgumentNullException(nameof(productAttributeMapping));

            searchModel.ProductAttributeMappingId = productAttributeMapping.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prepare segment search model
        /// </summary>
        /// <param name="searchModel">Segment search model</param>
        /// <returns>Segment search model</returns>
        public virtual ProductSegmentSearchModel PrepareSegmentSearchModel(ProductSegmentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available stores
            _baseAdminModelFactory.PrepareStores(searchModel.AvailableStores);

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged prodcut segment list model
        /// </summary>
        /// <param name="searchModel">Product segment search model</param>
        /// <returns>Product segment list model</returns>
        public virtual ProductSegmentListModel PrepareProductSegmentListModel(ProductSegmentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get product segment
            var segments = _productSegmentService.GetAllProductSegment(name : searchModel.SearchSegmentName,
                storeId: searchModel.SearchStoreId,
                pageIndex: searchModel.Page - 1, 
                pageSize: searchModel.PageSize);

            var model = new ProductSegmentListModel().PrepareToGrid(searchModel, segments, () =>
            {
                //fill in model values from the entity
                return segments.Select(segment =>
                {
                    var store = _storeService.GetStoreById(segment.StoreId);
                    return new ProductSegmentModel
                    {
                        Id = segment.Id,
                        Name = segment.Name,
                        Description = segment.Description,
                        DisplayOrder = segment.DisplayOrder,
                        StoreId = segment.StoreId,
                        StoreName = store?.Name ?? (segment.StoreId == 0 ? _localizationService.GetResource("Admin.Configuration.Settings.StoreScope.AllStores") : string.Empty)
                    };
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare product segment model
        /// </summary>
        /// <param name="model">Product segment model</param>
        /// <param name="productSegment">productSegment</param>
        /// <returns>ProductSegmentModel</returns>
        public virtual ProductSegmentModel PrepareProductSegmentModel(ProductSegmentModel model, PDD_ProductSegment productSegment)
        {
            if (productSegment != null)
            {
                //fill in model values from the entity
                var store = _storeService.GetStoreById(productSegment.StoreId);
                model =  new ProductSegmentModel
                {
                    Id = productSegment.Id,
                    Name = productSegment.Name,
                    Description = productSegment.Description,
                    DisplayOrder = productSegment.DisplayOrder,
                    StoreId = productSegment.StoreId,
                    StoreName = store?.Name ?? (productSegment.StoreId == 0 ? _localizationService.GetResource("Admin.Configuration.Settings.StoreScope.AllStores") : string.Empty)
                };
            }
            
            //prepare storelist
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Configuration.Settings.StoreScope.AllStores"), Value = "0" });
            foreach (var store in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = store.Name, Value = store.Id.ToString() });

            return model;
        }

        /// <summary>
        /// Prepare product attribute mapping model
        /// </summary>
        /// <param name="model">Product attribute mapping model</param>
        /// <param name="product">Product</param>
        /// <param name="productAttributeMapping">Product attribute mapping</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Product attribute mapping model</returns>
        public virtual Models.ProductAttributeMappingModel PrepareProductAttributeMappingModel(Models.ProductAttributeMappingModel model,
            int productSegmentId, int productAttributeId,ProductAttributeMapping productAttributeMapping, bool excludeProperties = false)
        {
            Action<ProductAttributeMappingLocalizedModel, int> localizedModelConfiguration = null;
            
            if (productAttributeMapping != null)
            {
                //fill in model values from the entity
                model = model ?? new Models.ProductAttributeMappingModel
                {
                    Id = productAttributeMapping.Id
                };

                model.ProductAttribute = _productAttributeService.GetProductAttributeById(productAttributeMapping.ProductAttributeId).Name;
                model.AttributeControlType = _localizationService.GetLocalizedEnum(productAttributeMapping.AttributeControlType);

                if (!excludeProperties)
                {
                    model.ProductAttributeId = productAttributeMapping.ProductAttributeId;
                    model.TextPrompt = productAttributeMapping.TextPrompt;
                    model.IsRequired = productAttributeMapping.IsRequired;
                    model.AttributeControlTypeId = productAttributeMapping.AttributeControlTypeId;
                    model.DisplayOrder = productAttributeMapping.DisplayOrder;
                    model.ValidationMinLength = productAttributeMapping.ValidationMinLength;
                    model.ValidationMaxLength = productAttributeMapping.ValidationMaxLength;
                    model.ValidationFileAllowedExtensions = productAttributeMapping.ValidationFileAllowedExtensions;
                    model.ValidationFileMaximumSize = productAttributeMapping.ValidationFileMaximumSize;
                    model.DefaultValue = productAttributeMapping.DefaultValue;
                }

                //prepare condition attributes model
                model.ConditionAllowed = true;
                PrepareProductAttributeConditionModel(model.ConditionModel, productAttributeMapping);

                //define localized model configuration action
                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.TextPrompt = _localizationService.GetLocalized(productAttributeMapping, entity => entity.TextPrompt, languageId, false, false);
                };

                //prepare nested search model
                PrepareProductAttributeValueSearchModel(model.ProductAttributeValueSearchModel, productAttributeMapping);
            }
            
            //prepare localized models
            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            //prepare available product attributes
            //model.AvailableProductAttributes = _productAttributeService.GetAllProductAttributes().Select(productAttribute => new SelectListItem
            //{
            //    Text = productAttribute.Name,
            //    Value = productAttribute.Id.ToString()
            //}).ToList();

            var attribute = _productAttributeService.GetProductAttributeById(productAttributeId);
            model.ProductAttributeId = attribute != null ?  attribute.Id : 0 ;
            model.ProductAttribute = attribute != null ? attribute.Name : "";
            model.ProductSegmentId = productSegmentId;
            
            return model;
        }

        /// <summary>
        /// Prepare product attribute value model
        /// </summary>
        /// <param name="model">Product attribute value model</param>
        /// <param name="productAttributeMapping">Product attribute mapping</param>
        /// <param name="productAttributeValue">Product attribute value</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Product attribute value model</returns>
        public virtual Models.ProductAttributeValueModel PrepareProductAttributeValueModel(Models.ProductAttributeValueModel model,
            ProductAttributeMapping productAttributeMapping, ProductAttributeValue productAttributeValue, bool excludeProperties = false)
        {
            if (productAttributeMapping == null)
                throw new ArgumentNullException(nameof(productAttributeMapping));

            Action<ProductAttributeValueLocalizedModel, int> localizedModelConfiguration = null;

            if (productAttributeValue != null)
            {
                //fill in model values from the entity
                model = model ?? new Models.ProductAttributeValueModel
                {
                    ProductAttributeMappingId = productAttributeValue.ProductAttributeMappingId,
                    AttributeValueTypeId = productAttributeValue.AttributeValueTypeId,
                    AttributeValueTypeName = _localizationService.GetLocalizedEnum(productAttributeValue.AttributeValueType),
                    AssociatedProductId = productAttributeValue.AssociatedProductId,
                    Name = productAttributeValue.Name,
                    ColorSquaresRgb = productAttributeValue.ColorSquaresRgb,
                    DisplayColorSquaresRgb = productAttributeValue
                        .ProductAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares,
                    ImageSquaresPictureId = productAttributeValue.ImageSquaresPictureId,
                    DisplayImageSquaresPicture = productAttributeValue
                        .ProductAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares,
                    PriceAdjustment = productAttributeValue.PriceAdjustment,
                    PriceAdjustmentUsePercentage = productAttributeValue.PriceAdjustmentUsePercentage,
                    WeightAdjustment = productAttributeValue.WeightAdjustment,
                    Cost = productAttributeValue.Cost,
                    CustomerEntersQty = productAttributeValue.CustomerEntersQty,
                    Quantity = productAttributeValue.Quantity,
                    IsPreSelected = productAttributeValue.IsPreSelected,
                    DisplayOrder = productAttributeValue.DisplayOrder,
                    PictureId = productAttributeValue.PictureId
                };

                model.AssociatedProductName = _productService.GetProductById(productAttributeValue.AssociatedProductId)?.Name;

                //define localized model configuration action
                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Name = _localizationService.GetLocalized(productAttributeValue, entity => entity.Name, languageId, false, false);
                };
            }

            model.ProductAttributeMappingId = productAttributeMapping.Id;
            model.DisplayColorSquaresRgb = productAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares;
            model.DisplayImageSquaresPicture = productAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares;

            //set default values for the new model
            if (productAttributeValue == null)
                model.Quantity = 1;

            //prepare localized models
            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            //prepare picture models
            var productPictures = _productService.GetProductPicturesByProductId(productAttributeMapping.ProductId);
            model.ProductPictureModels = productPictures.Select(productPicture => new ProductPictureModel
            {
                Id = productPicture.Id,
                ProductId = productPicture.ProductId,
                PictureId = productPicture.PictureId,
                PictureUrl = _pictureService.GetPictureUrl(productPicture.PictureId),
                DisplayOrder = productPicture.DisplayOrder
            }).ToList();
            
            return model;
        }

        /// <summary>
        /// Prepare paged product attribute value list model
        /// </summary>
        /// <param name="searchModel">Product attribute value search model</param>
        /// <param name="productAttributeMapping">Product attribute mapping</param>
        /// <returns>Product attribute value list model</returns>
        //public virtual ProductAttributeValueListModel PrepareProductAttributeValueListModel(ProductAttributeValueSearchModel searchModel,
        //    ProductAttributeMapping productAttributeMapping)
        //{
        //    if (searchModel == null)
        //        throw new ArgumentNullException(nameof(searchModel));

        //    if (productAttributeMapping == null)
        //        throw new ArgumentNullException(nameof(productAttributeMapping));

        //    //get product attribute values
        //    var productAttributeValues = _productAttributeService
        //        .GetProductAttributeValues(productAttributeMapping.Id).ToPagedList(searchModel);

        //    //prepare list model
        //    var model = new ProductAttributeValueListModel().PrepareToGrid(searchModel, productAttributeValues, () =>
        //    {
        //        return productAttributeValues.Select(value =>
        //        {
        //            //fill in model values from the entity
        //            var productAttributeValueModel = new Models.ProductAttributeValueModel
        //            {
        //                Id = value.Id,
        //                ProductAttributeMappingId = value.ProductAttributeMappingId,
        //                AttributeValueTypeId = value.AttributeValueTypeId,
        //                AssociatedProductId = value.AssociatedProductId,
        //                ColorSquaresRgb = value.ColorSquaresRgb,
        //                ImageSquaresPictureId = value.ImageSquaresPictureId,
        //                PriceAdjustment = value.PriceAdjustment,
        //                PriceAdjustmentUsePercentage = value.PriceAdjustmentUsePercentage,
        //                WeightAdjustment = value.WeightAdjustment,
        //                Cost = value.Cost,
        //                CustomerEntersQty = value.CustomerEntersQty,
        //                Quantity = value.Quantity,
        //                IsPreSelected = value.IsPreSelected,
        //                DisplayOrder = value.DisplayOrder,
        //                PictureId = value.PictureId
        //            };

        //            //fill in additional values (not existing in the entity)
        //            productAttributeValueModel.AttributeValueTypeName = _localizationService.GetLocalizedEnum(value.AttributeValueType);
        //            productAttributeValueModel.Name = value.ProductAttributeMapping.AttributeControlType != AttributeControlType.ColorSquares
        //                ? value.Name : $"{value.Name} - {value.ColorSquaresRgb}";
        //            if (value.AttributeValueType == AttributeValueType.Simple)
        //            {
        //                productAttributeValueModel.PriceAdjustmentStr = value.PriceAdjustment.ToString("G29");
        //                if (value.PriceAdjustmentUsePercentage)
        //                    productAttributeValueModel.PriceAdjustmentStr += " %";
        //                productAttributeValueModel.WeightAdjustmentStr = value.WeightAdjustment.ToString("G29");
        //            }

        //            if (value.AttributeValueType == AttributeValueType.AssociatedToProduct)
        //            {
        //                productAttributeValueModel
        //                    .AssociatedProductName = _productService.GetProductById(value.AssociatedProductId)?.Name ?? string.Empty;
        //            }

        //            var pictureThumbnailUrl = _pictureService.GetPictureUrl(value.PictureId, 75, false);
        //            //little hack here. Grid is rendered wrong way with <img> without "src" attribute
        //            if (string.IsNullOrEmpty(pictureThumbnailUrl))
        //                pictureThumbnailUrl = _pictureService.GetPictureUrl(null, 1);
        //            productAttributeValueModel.PictureThumbnailUrl = pictureThumbnailUrl;

        //            return productAttributeValueModel;
        //        });
        //    });


        //    return model;
        //}

        /// <summary>
        /// Prepare paged product specification attribute list model
        /// </summary>
        /// <param name="searchModel">Product specification attribute search model</param>
        /// <param name="product">Product</param>
        /// <returns>Product specification attribute list model</returns>
        public virtual Models.ProductSpecificationAttributeListModel PrepareProductSpecificationAttributeListModel(
            Models.ProductSpecificationAttributeModel searchModel, IList<Product> products)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (products == null)
                throw new ArgumentNullException(nameof(products));

            var productSpecificationAttributes = new List<ProductSpecificationAttribute>();
            //get product specification attributes
            for (int i = 0; i < products.Count; i++)
            {
                productSpecificationAttributes.AddRange(_specificationAttributeService.GetProductSpecificationAttributes(products[i].Id));
            }

            productSpecificationAttributes = productSpecificationAttributes.Where(p => p.SpecificationAttributeOption.SpecificationAttributeId == searchModel.ProductSpecificationId).ToList();

            bool isRecordAdd = true;
            List<int> PDD_ProductAttributeMapIdList = new List<int>();
            var productSpecificationAttributeModelList = new List<Models.ProductSpecificationAttributeModel>();
            foreach (var attribute in productSpecificationAttributes)
            {
                var specificationMapper = _productFilterOptionService.GetProductAttributeMapWithSegmentByAttributeMapperId(attribute.Id, searchModel.ProductSpecificationId, Domain.Enums.EntityTypeEnum.ProductSpecificationMapValue, searchModel.ProductSegmentId);

                var PDD_ProductAttributeMapId = 0;

                if (specificationMapper != null)
                {
                    if (!PDD_ProductAttributeMapIdList.Contains(specificationMapper.Id))
                    {
                        isRecordAdd = true;
                        PDD_ProductAttributeMapId = specificationMapper.Id;
                        PDD_ProductAttributeMapIdList.Add(specificationMapper.Id);
                    }

                }
                else
                    isRecordAdd = true;

                if (isRecordAdd)
                {
                    isRecordAdd = false;
                    //fill in model values from the entity
                    var productSpecificationAttributeModel = new Models.ProductSpecificationAttributeModel
                    {
                        Id = attribute.Id,
                        AttributeTypeId = attribute.AttributeTypeId,
                        AllowFiltering = attribute.AllowFiltering,
                        ShowOnProductPage = attribute.ShowOnProductPage,
                        DisplayOrder = attribute.DisplayOrder,
                        ProductSegmentId = searchModel.ProductSegmentId,
                        ProductSpecificationId = searchModel.ProductSpecificationId,
                        PDD_ProductAttributeMapId = PDD_ProductAttributeMapId
                    };

                    //fill in additional values (not existing in the entity)
                    productSpecificationAttributeModel.AttributeTypeName = _localizationService.GetLocalizedEnum(attribute.AttributeType);
                    productSpecificationAttributeModel.AttributeId = attribute.SpecificationAttributeOption.SpecificationAttribute.Id;
                    productSpecificationAttributeModel.AttributeName = attribute.SpecificationAttributeOption.SpecificationAttribute.Name;
                    switch (attribute.AttributeType)
                    {
                        case SpecificationAttributeType.Option:
                            productSpecificationAttributeModel.ValueRaw = WebUtility.HtmlEncode(attribute.SpecificationAttributeOption.Name);
                            productSpecificationAttributeModel.SpecificationAttributeOptionId = attribute.SpecificationAttributeOptionId;
                            break;
                        case SpecificationAttributeType.CustomText:
                            productSpecificationAttributeModel.ValueRaw = WebUtility.HtmlEncode(attribute.CustomValue);
                            break;
                        case SpecificationAttributeType.CustomHtmlText:
                            productSpecificationAttributeModel.ValueRaw = WebUtility.HtmlEncode(attribute.CustomValue);
                            break;
                        case SpecificationAttributeType.Hyperlink:
                            productSpecificationAttributeModel.ValueRaw = attribute.CustomValue;
                            break;
                    }
                    productSpecificationAttributeModelList.Add(productSpecificationAttributeModel);
                }
            }

            //prepare grid model
            var model = new Models.ProductSpecificationAttributeListModel
            {
                Data = productSpecificationAttributeModelList,
                Total = productSpecificationAttributeModelList.Count
            };

            return model;
        }
        #endregion
    }
}
