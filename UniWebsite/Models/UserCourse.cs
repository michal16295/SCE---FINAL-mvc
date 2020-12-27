using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SCE___FINAL.Models
{
    public class UserCourse
    {
        [Key, Column(Order = 0)]
        public string UserId { get; set; }
        [Key, Column(Order = 1)]
        public int CourseId { get; set; }
        public Nullable<int> Grade { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Course Course { get; set; }
    }
}