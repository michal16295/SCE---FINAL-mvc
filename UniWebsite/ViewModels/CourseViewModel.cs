using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace SCE___FINAL.ViewModels
{
    public class CourseViewModel : IValidatableObject
    {
        [Key]
        public int Courseid { get; set; }

        public string Name { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.DateTime)]
        public DateTime FinishDate { get; set; }

        public string Class { get; set; }

        public Nullable<int> Hours { get; set; }

        public string Lecturer { get; set; }

        [Display(Name = "Moed A")]
        [DataType(DataType.DateTime)]
        public Nullable<DateTime> MoedA { get; set; }

        [Display(Name = "Moed B")]
        [DataType(DataType.DateTime)]
        public Nullable<DateTime> MoedB { get; set; }

        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            if (MoedB < MoedA)
            {
                yield return new ValidationResult("Moed B must be after Moed A");
            }

        }

    }
}