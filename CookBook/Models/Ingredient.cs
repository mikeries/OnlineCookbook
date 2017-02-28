using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CookBook.Models
{
    public partial class Ingredient
    {

        public int IngredientID { get; set; }
        public int RecipeID { get; set; }

        [StringLength(5)]
        public string NDB_No { get; set; }

        public double? Quantity { get; set; }

        public int SortIndex { get; set; }

        [StringLength(10)]
        public string UnitOfMeasurement { get; set; }

        [StringLength(2)]
        public string MeasurementSeq { get; set; }

        [StringLength(50)]
        public string Description { get; set; }

    }
}