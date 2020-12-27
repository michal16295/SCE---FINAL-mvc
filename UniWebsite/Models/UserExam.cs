using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SCE___FINAL.Models
{
    public class UserExam : IValidatableObject
    {
        [Key, Column(Order = 0)]
        public string UserId { get; set; }
        [Key, Column(Order = 1)]
        public int ExamId { get; set; }

        public Nullable<int> Grade { get; set; }

        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            if (Grade < 0 || Grade > 100)
            {
                yield return new ValidationResult("Grade Must be between 0 - 100");
            }

        }
    }
}