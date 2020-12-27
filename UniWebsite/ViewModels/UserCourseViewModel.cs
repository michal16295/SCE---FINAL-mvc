using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SCE___FINAL.ViewModels
{
    public class UserCourseViewModel
    {
        [Key]
        public int CourseId { get; set; }
        public string Name { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.DateTime)]
        public DateTime FinishDate { get; set; }

        [Display(Name = "Moed A")]
        [DataType(DataType.DateTime)]
        public Nullable<DateTime> MoedA { get; set; }

        [Display(Name = "Moed B")]
        [DataType(DataType.DateTime)]
        public Nullable<DateTime> MoedB { get; set; }

        public string Class { get; set; }

        public Nullable<int> Hours { get; set; }

        [Display(Name = "Final Grade")]
        public Nullable<int> FinalGrade { get; set; }

        public string Lecturer { get; set; }

        public bool AppliedMoedB { get; set; }
    }
}