﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KTechTemplateDAL.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace KTechTemplate.ViewModels
{
    public class ProductViewModel
    {
        public Product? Product { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> CategoryList { get; set; }
    }
}
