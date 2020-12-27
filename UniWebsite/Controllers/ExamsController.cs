using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SCE___FINAL.Models;
using System.Diagnostics;
using SCE___FINAL.Services;
using System.Threading.Tasks;
using SCE___FINAL.ViewModels;
using Microsoft.AspNet.Identity;

namespace SCE___FINAL.Controllers
{
    [Authorize]
    public class ExamsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private CourseServices courseService = new CourseServices();
        private ExamServices examService = new ExamServices();

        // GET: Exams
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var exam = db.Exam.Include(e => e.Course);
            return View(exam.ToList());
        }

        // GET: MyExams
        public ActionResult MyExams()
        {
            var currentUser = User.Identity.GetUserId();
            var userExam = db.UserExam.Where(e => e.UserId == currentUser).ToList();
            List<ExamViewModel> exams = new List<ExamViewModel>();
            foreach(var ue in userExam)
            {
                var exam = db.Exam.Where(e => e.ExamId == ue.ExamId).FirstOrDefault();
                var course = db.Courses.Find(exam.CourseId);
                var examViewModel = new ExamViewModel
                {
                    ExamId = exam.ExamId,
                    course = course.Name,
                    StartDate = exam.Date,
                    EndDate = exam.Date.AddHours(exam.Hours),
                    Class = exam.Class,
                    Hours = exam.Hours,
                    Moed = exam.Moed
                    
                };
                if (ue.Grade != null) examViewModel.Grade = ue.Grade;   
                exams.Add(examViewModel);
            }
            return View(exams.ToList());
        }


        // GET: Exams/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create(int CourseId, string Moed)
        {
            
            return View();
        }

        // POST: Exams/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int CourseId, string Moed, [Bind(Include = "ExamId,CourseId,Moed,Date,Class,Hours")] Exam exam)
        {
            if (ModelState.IsValid)
            {
                if (!examService.validateDate(exam.Date, CourseId))
                {
                    ModelState.AddModelError("Date", "The Exam must be after the course ends");
                    return View();
                }
                if (!courseService.isClassFree(exam.Class, exam.Date, exam.Hours))
                {
                    ModelState.AddModelError("Class", "This class is taken");
                    return View();
                }
                if (exam.Moed == "B")
                {
                    if (!examService.validateMoedBDate(exam.Date, CourseId))
                    {
                        ModelState.AddModelError("Moed", "Moed B needs to be after Moed A");
                        return View();
                    }

                }
                db.Exam.Add(exam);
                db.SaveChanges();
                examService.assignUsersToExam(exam);
                
                return RedirectToAction("Details", "Courses", new { id = CourseId });
            }

           return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Calender()
        {
            return View();
        }
        public async Task<JsonResult> GetEvents()
        {
            var exams = await db.Exam.ToListAsync();
            List<ExamViewModel> examViewModel = new List<ExamViewModel>();
            foreach (var exam in exams)
            {
                examService.getExamViewModel(exam ,examViewModel);

            }
            return new JsonResult { Data = examViewModel, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }

        [HttpGet]
        public ActionResult MyExamsCalendar()
        {
            return View();
        }
        public JsonResult GetExams()
        {
            var currUser = User.Identity.GetUserId();
            var userExam = db.UserExam.Where(e => e.UserId == currUser).ToList();

            List<ExamViewModel> examViewModel = new List<ExamViewModel>();
            foreach (var ue in userExam)
            {
                var exam = db.Exam.Where(e => e.ExamId == ue.ExamId).FirstOrDefault();
                examService.getExamViewModel(exam, examViewModel);

            }
            return new JsonResult { Data = examViewModel, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }



        // GET: Exams/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id , string Moed)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            Exam exam = db.Exam.Where(e => e.CourseId == id && e.Moed == Moed).FirstOrDefault();
            if (exam == null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            ViewBag.CourseId = new SelectList(db.Courses, "Courseid", "Name", exam.CourseId);
            return View(exam);
        }

        // POST: Exams/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ExamId,CourseId,Moed,Date,Class,Hours")] Exam exam)
        {
            if (ModelState.IsValid)
            {
                var course = db.Courses.Find(exam.CourseId);
                if(exam.Moed == "B")
                {
                    var examA = db.Exam.Where(e => e.CourseId == exam.CourseId && e.Moed == "A").FirstOrDefault();
                    if(exam.Date < examA.Date)
                    {
                        ModelState.AddModelError("Date", "Moed B is after Moed A");
                        return View(exam);
                    }
                }
                
                if(course.FinishDate > exam.Date)
                {
                    ModelState.AddModelError("Date", "Exam must be after the course ends");
                    return View(exam);
                }
                db.Entry(exam).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CourseId = new SelectList(db.Courses, "Courseid", "Name", exam.CourseId);
            return View(exam);
        }

        [Authorize(Roles = "Admin, Lecturer")]
        [HttpGet]
        public ActionResult UpdateGrades(int CourseId, string UserId, string Moed, string Name)
        {
            var ExamId = db.Exam.Where(e => e.CourseId == CourseId && e.Moed == Moed).Select(e=> e.ExamId).FirstOrDefault();
            var course = db.Courses.Find(CourseId);
            UserExam userExam = db.UserExam.Find(UserId, ExamId);
            if (userExam == null)
            {
                ModelState.AddModelError("UserId", "The user didnt applied for moed B");
                return RedirectToAction("Details", "Courses", new { id = CourseId });
            }
            ViewBag.Name = Name;
            ViewBag.Course = course.Name;
            return View(userExam);
        }
        [Authorize(Roles = "Admin, Lecturer")]
        [HttpPost]
        public ActionResult UpdateGrades(int CourseId, [Bind(Include = "UserId,ExamId,Grade")] UserExam userExam)
        {
            //Only course lecturer or admin can change the grades
            var currUser = db.Users.Find(User.Identity.GetUserId());
            if(currUser.Role == "Lecturer")
            {
                var lecturerCourse = db.UserCourse.Find(currUser.Id, CourseId);
                if(lecturerCourse == null)
                {
                    ModelState.AddModelError("Grade", "Only Course Lecturer can Assign Grades");
                    return View();
                }
            }
            var exam = db.Exam.Where(e => e.ExamId == userExam.ExamId).FirstOrDefault();
            var userCourse = db.UserCourse.Find(userExam.UserId, CourseId);
            if (ModelState.IsValid)
            {

                DateTime currentDate = DateTime.Now;
                if (exam.Date > currentDate)
                {
                    ModelState.AddModelError("Grade", "Cant assign grades before the exam");
                    return View();
                }
                //Final grade is Moed B
                if (userCourse.Grade == null)
                {
                    userCourse.Grade = userExam.Grade;
                    db.Entry(userCourse).State = EntityState.Modified;
                }
                if (userCourse.Grade != null && exam.Moed == "B")
                {
                    userCourse.Grade = userExam.Grade;
                    db.Entry(userCourse).State = EntityState.Modified;
                }

                db.Entry(userExam).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", "Courses", new { id = CourseId });
            }

            return View();
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


        
    }
}
