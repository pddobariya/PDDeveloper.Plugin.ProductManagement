using System;
using System.Collections.Generic;
using System.Text;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace PDDeveloper.Plugin.ProductManagement.Models
{
    public class ProductSegmentAddSpecificationAttributeModel : AddSpecificationAttributeModel
    {
        public ProductSegmentAddSpecificationAttributeModel()
        {
        }

        [NopResourceDisplayName("Admin.Catalog.Products.SpecificationAttributes.Fields.SpecificationAttribute")]
        
        public string SpecificationAttributeName { get; set; }
        public int ProductSegmentId { get; set; }
        public int SpecificationAttributeId { get; set; }
        public int PDD_ProductAttributeMapId { get; set; }
    }
}
