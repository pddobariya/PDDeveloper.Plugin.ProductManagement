﻿using Nop.Web.Areas.Admin.Models.Catalog;

namespace PDDeveloper.Plugin.ProductManagement.Models
{
    public class ProductSpecificationAttributeModel : Nop.Web.Areas.Admin.Models.Catalog.ProductSpecificationAttributeModel
    {
        public ProductSpecificationAttributeModel()
        {
            AddSpecificationAttributeModel = new AddSpecificationAttributeToProductModel();
            ProductSpecificationAttributeSearchModel = new ProductSpecificationAttributeSearchModel();
        }

        //add specification attribute model
        public AddSpecificationAttributeToProductModel AddSpecificationAttributeModel { get; set; }

        public ProductSpecificationAttributeSearchModel ProductSpecificationAttributeSearchModel { get; set; }
        
        public int ProductSegmentId { get; set; }

        public int ProductSpecificationId { get; set; }

        public int PDD_ProductAttributeMapId { get; set; }
    }
}
