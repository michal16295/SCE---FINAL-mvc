using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 
namespace SCE___FINAL.Models
{
    public class Exam : IValidatableObject
    {
        [Key, Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExamId { get; set; }

        [Key, Column(Order = 1)]
        public int CourseId { get; set; }

        [Key, Column(Order = 2)]
        public string Moed { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; }

        public string Class { get; set; }

        public int Hours { get; set; }

        public virtual ICollection<UserExam> UserExam { get; set; }
        public virtual Course Course { get; set; }

        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            if (Moed != "A" && Moed != "B")
            {
                yield return new ValidationResult("The Exam has only Moed A and Moed B");
            }
            if (Hours < 0)
            {
                yield return new ValidationResult("Hours Must be Greater than 0");
            }

        }
    }
}