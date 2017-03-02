using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CookBook.Models
{
    public partial class Recipe
    {

        public int RecipeID { get; set; }

        public string UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual ApplicationUser User { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        public int? Servings { get; set; }

        public double? PreperationTime { get; set; }

        public double? InactiveTime { get; set; }

        public double? CookTime { get; set; }

        public string Comments { get; set; }

        public virtual List<Ingredient> Ingredients { get; set; }
        public virtual List<Procedure> Procedures { get; set; }
    }
}