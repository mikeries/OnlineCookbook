using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CookBook.Models
{
    public class Procedure
    {
        public int ProcedureID { get; set; }

        [Required]
        public int RecipeID { get; set; }

        [Required]
        [StringLength(200)]
        public string StepText { get; set; }

        public int StepOrder { get; set; }

    }
}