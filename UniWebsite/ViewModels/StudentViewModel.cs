using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;

namespace SCE___FINAL.ViewModels
{
    public class StudentViewModel
    {
        public string id { get; set; }
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Display(Name = "Username")]
        public string UserName { get; set; }
        public string Email { get; set; }
        public int CourseId { get; set; }
        public Nullable<int> MoedAGrade { get; set; }
        public Nullable<int> MoedBGrade { get; set; }
        public Nullable<int> FinalGrade { get; set; }
    }
}