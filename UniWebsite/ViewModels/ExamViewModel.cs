using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SCE___FINAL.ViewModels
{
    public class ExamViewModel
    {
        [Key]
        public int ExamId { get; set; }
        public string course { get; set; }

        public string Moed { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

        public string Class { get; set; }

        public int Hours { get; set; }

        public Nullable<int> Grade { get; set; }
    }
}