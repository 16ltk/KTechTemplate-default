using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTechTemplateDAL.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public virtual string Name { get; set; }

        [Display(Name = "Display Order")]
        [Range(1, 2000, ErrorMessage = "Display Order must be between 1 and 2000 only.")]
        public int DisplayOrder { get; set; }

        public DateTime CreatedDateTime { get; set; } = DateTime.Now;

        public ICollection<Product>? Products { get; set; } //For future usage: Can be used for retrieving a Specific Category with its Products
    }
}
