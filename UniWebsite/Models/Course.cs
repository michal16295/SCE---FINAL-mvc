using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SCE___FINAL.Models
{
    public class Course : IValidatableObject
    {
  
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]

        public int Courseid { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.DateTime)]
        public DateTime FinishDate { get; set; }

        [Required]
        public string Class { get; set; }

        public Nullable<int> Hours { get; set; }

        public virtual ICollection<UserCourse> UserCourse { get; set; }





        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            if (FinishDate < StartDate)
            {
                yield return new ValidationResult("End Date must be greater than Start Date");
            }
            if(Hours <= 0)
            {
                yield return new ValidationResult("Hours must be greater than 0");
            }
            
        }
    }
}