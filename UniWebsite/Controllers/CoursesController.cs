using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SCE___FINAL.Models;
using SCE___FINAL.ViewModels;
using System.Diagnostics;
using System.Threading.Tasks;
using SCE___FINAL.Services;

namespace SCE___FINAL.Controllers
{
    [Authorize]
    public class CoursesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private CourseServices courseService = new CourseServices();
        private ExamServices examService = new ExamServices();


        // GET: Courses
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return View(db.Courses.ToList());
        }
        // GET: AddLecturer
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult AddLecturer(int CourseId)
        {
            ViewBag.UserId = new SelectList(db.Users.Where(u => u.Role == "Lecturer"), "Id", "Username");
            ViewBag.CourseId = new SelectList(db.Courses.Where(c => c.Courseid == CourseId), "Courseid", "Name", CourseId);
            return View();
        }
        // GET: AddLecturer
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult AddLecturer(string UserId, int CourseId, [Bind(Include = "UserId,CourseId")] UserCourse userCourse)
        {
            ViewBag.UserId = new SelectList(db.Users.Where(u => u.Role == "Lecturer"), "Id", "Username");
            ViewBag.CourseId = new SelectList(db.Courses.Where(c => c.Courseid == CourseId), "Courseid", "Name", CourseId);
            if (ModelState.IsValid)
            {
                Course course = db.Courses.Find(userCourse.CourseId);

                if (courseService.checkUsersHours(course, UserId))
                {
                    ModelState.AddModelError("UserId", "Lecturer has a class at the same time");
                    return View();
                }
                var exams = db.Exam.Where(e => e.CourseId == userCourse.CourseId).ToArray();
                db.UserCourse.Add(userCourse);
                if (exams != null)
                {
                    foreach(var i in exams)
                    {
                        var userExam = new UserExam { UserId = userCourse.UserId, ExamId = i.ExamId };
                        db.UserExam.Add(userExam);

                    }
                }

                db.SaveChanges();
                return RedirectToAction("Details", "Courses", new { id = userCourse.CourseId });
            }
            return View(userCourse);
        }
        // GET: AddStudent
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult AddStudent(int CourseId)
        {
            ViewBag.UserId = new SelectList(db.Users.Where(u => u.Role == "Student"), "Id", "Username");
            ViewBag.CourseId = new SelectList(db.Courses.Where(c => c.Courseid == CourseId), "Courseid", "Name", CourseId);

            return View();
        }

        // POST: AddStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult AddStudent(string UserId, int CourseId, [Bind(Include = "UserId,CourseId")] UserCourse userCourse)
        {
            ViewBag.UserId = new SelectList(db.Users.Where(u => u.Role == "Student"), "Id", "Username");
            ViewBag.CourseId = new SelectList(db.Courses.Where(c => c.Courseid == CourseId), "Courseid", "Name", CourseId);

            if (ModelState.IsValid)
            {
                UserCourse uc = db.UserCourse.Find(userCourse.UserId, userCourse.CourseId);
                Course course = db.Courses.Find(userCourse.CourseId);
                if (uc != null)
                {
                    ModelState.AddModelError("UserId", "The student already in this course");
                    return View();
                }
              
                if (courseService.checkUsersHours(course, UserId))
                {
                    ModelState.AddModelError("UserId", "Student has a class at the same time");
                    return View();
                }
                var exam = db.Exam.Where(e => e.CourseId == userCourse.CourseId && e.Moed == "A").FirstOrDefault();
                db.UserCourse.Add(userCourse);
                if (exam != null)
                {
                    var userExam = new UserExam { UserId = userCourse.UserId, ExamId = exam.ExamId };
                    db.UserExam.Add(userExam);
                }

                db.SaveChanges();
                return RedirectToAction("Details", "Courses", new { id = userCourse.CourseId });
            }
            return View(userCourse);
        }

        [HttpGet]
        public ActionResult Calender()
        {
            return View();
        }
        public async Task<JsonResult> GetEvents()
        {

            var courses = await db.Courses.ToListAsync();
            List<CourseViewModel> cm = new List<CourseViewModel>();
            foreach (var course in courses)
            {
                courseService.getCourseViewModel(course, cm);
               
            }
            return new JsonResult { Data = cm, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }

        // GET: Courses/Details/5
        [Authorize(Roles = "Admin,Lecturer")]
        public ActionResult Details(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Course course = db.Courses.Find(id);
               if (course == null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            string lecturer = courseService.getLecturer(course.Courseid);
            CourseViewModel vm = courseService.GetCourseDetails(id, course, lecturer);
            return View(vm);

            
        }

        // GET: Courses/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            ViewBag.UsersId = new SelectList(db.Users.Where(u => u.Role == "Lecturer"), "Id", "Username");
            return View();
        }

        // POST: Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include = "Courseid,Name,StartDate,FinishDate,Class,Hours")] Course course, string UsersId)
        {
            ViewBag.UsersId = new SelectList(db.Users.Where(u => u.Role == "Lecturer"), "Id", "Username");
            if (ModelState.IsValid)
            {
                if(UsersId != "" && UsersId != null)
                {
                    if (courseService.checkUsersHours(course, UsersId)){
                        ModelState.AddModelError("StartDate", "Lecturer has a class at the same time");
                        return View(course);
                    }
                    UserCourse uc = new UserCourse
                    {
                        UserId = UsersId,
                        CourseId = course.Courseid
                    };
                    db.UserCourse.Add(uc);
                }
                if (!courseService.isClassFree(course.Class, course.StartDate, course.Hours))
                    {
                        ModelState.AddModelError("Class", "This class is taken");
                        return View(course);
                    }
                db.Courses.Add(course);
                db.SaveChanges();
                return RedirectToAction("Index");

            }

            return View(course);
        }

        // GET: Courses/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Course course = db.Courses.Find(id);
 
            if (course == null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            return View(course);
        }

        // POST: Courses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "Courseid,Name,StartDate,FinishDate,Class,Hours")] Course course)
        {
            var currCourse = db.Courses.Find(course.Courseid);
            if (ModelState.IsValid)
            {
                var newContext = new ApplicationDbContext();
                var exams = db.Exam.Where(e => e.CourseId == course.Courseid).ToArray();
                foreach(var exam in exams)
                {
                    if(course.FinishDate > exam.Date || course.StartDate > exam.Date)
                    {
                        ModelState.AddModelError("FinishDate", "Course Must be Before Exam");
                        return View(course);
                    }
                }
                if (!courseService.isClassFree(course.Class, course.StartDate, course.Hours) && currCourse.Class != course.Class)
                {
                    ModelState.AddModelError("Class", "This class is taken");
                    return View(course);
                }
                newContext.Entry(course).State = EntityState.Modified;
                newContext.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(course);
        }

        // GET: Courses/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Course course = db.Courses.Find(id);
            if (course == null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            return View(course);
        }

        // POST: Courses/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            courseService.deleteCourse(id);
            return RedirectToAction("Index");
        }

        //GET: Studets
        //Returns a list of students in the gives course
        public ActionResult Students(int id)
        {
            var users = courseService.StudentsList(id);
            return PartialView(users.ToList());

        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult DeleteStudent(string UserId, int CourseId)
        {
            var userCourse = db.UserCourse.Find(UserId, CourseId);
            if (userCourse == null) return RedirectToAction("NotFound", "Home");
            db.UserCourse.Remove(userCourse);

            var userExam = db.UserExam.Where(u => u.UserId == UserId).ToList();
            if (userExam == null) return RedirectToAction("NotFound", "Home");
            foreach (var exam in userExam)
            {
                db.UserExam.Remove(exam);
            }
            db.SaveChanges();
            return RedirectToAction("Details", new { id = CourseId });
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
