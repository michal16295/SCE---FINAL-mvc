using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SCE___FINAL.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Threading.Tasks;
using System.Data.Entity.Validation;
using System.Diagnostics;
using SCE___FINAL.ViewModels;
using SCE___FINAL.Services;

namespace SCE___FINAL.Controllers
{
   [Authorize]
    public class UsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private CourseServices courseService = new CourseServices();

        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        // GET: ApplicationUsers
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return View(db.Users.ToList());
        }

        // GET: ApplicationUsers/Students
        [Authorize(Roles = "Admin")]
        public ActionResult Students()
        {
            return View(db.Users.Where(u => u.Role == "Student").ToList());
        }

        // GET: ApplicationUsers/Lecturers
        [Authorize(Roles = "Admin")]
        public ActionResult Lecturers()
        {
            return View(db.Users.Where(u => u.Role == "Lecturer").ToList());
        }

        //GET : Courses
        [Authorize(Roles = "Admin,Student")]
        public ActionResult courses(string id)
        {
           
            var userCourses = db.UserCourse.Where(e => e.UserId == id).ToArray();
            List<UserCourseViewModel> userCoursesViewModel = new List<UserCourseViewModel>();
            foreach(var userCourse in userCourses)
            {
                var course = db.Courses.Find(userCourse.CourseId);
                var MoedA = db.Exam.Where(e => e.CourseId == course.Courseid && e.Moed == "A").FirstOrDefault();
                var MoedB = db.Exam.Where(e => e.CourseId == course.Courseid && e.Moed == "B").FirstOrDefault();
                var uc = new UserCourseViewModel
                {
                    CourseId = course.Courseid,
                    Name = course.Name,
                    StartDate = course.StartDate,
                    FinishDate = course.FinishDate,
                    Hours = course.Hours,
                    Class = course.Class,
                    FinalGrade = userCourse.Grade,
                    Lecturer = courseService.getLecturer(course.Courseid),
                    AppliedMoedB = false
                    

                };
                if (MoedA != null) uc.MoedA = MoedA.Date;
                if (MoedB != null)
                {
                    uc.MoedB = MoedB.Date;
                    var userExam = db.UserExam.Where(e => e.UserId == id && e.ExamId == MoedB.ExamId).FirstOrDefault();
                    if(userExam != null) uc.AppliedMoedB = true;
                }

                userCoursesViewModel.Add(uc);
            }
            return PartialView(userCoursesViewModel.ToList());
        }


        // GET: ApplicationUsers/Details/5
        public ActionResult Details(string id)
        {
            var currentUser = db.Users.Find(User.Identity.GetUserId());
            if(currentUser.Role != "Admin" && currentUser.Id != id)
            {
                return RedirectToAction("NotAuthorized", "Home");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser applicationUser = db.Users.Find(id);
            if (applicationUser == null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            
            return View(applicationUser);
        }

        // GET: ApplicationUsers/Create
       [Authorize(Roles = "Admin")]
        public ActionResult Create(string Role)
        {
            return View();
        }

        // POST: ApplicationUsers/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if(model.Role != "Student" && model.Role != "Admin" && model.Role != "Lecturer")
                {
                    ModelState.AddModelError("Role", "Role must be either: Admin, Lecturer or Student");
                    return View(model);
                }
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Role = model.Role
                };

                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {

                    db.Users.Add(user);
                    if (model.Role == "Student") return RedirectToAction("Students");
                    else if (model.Role == "Lecturer") return RedirectToAction("Lecturers");
                    else return RedirectToAction("Index");

                }
                AddErrors(result);
            }
            return View(model);
        }

        // GET: ApplicationUsers/Edit/5

        public ActionResult Edit(string id)
        {
            var currUser = db.Users.Find(User.Identity.GetUserId());
            if(currUser.Role != "Admin" && currUser.Id != id)
            {
                return RedirectToAction("NotAuthorized", "Home");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser applicationUser = db.Users.Find(id);
            if (applicationUser == null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            return View(applicationUser);
        }

        // POST: ApplicationUsers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,FirstName,LastName,Role,Email,EmailConfirmed,PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName")] ApplicationUser applicationUser)
        {
            var currUser = db.Users.Find(User.Identity.GetUserId());
            if (ModelState.IsValid)
            {
                var newContext = new ApplicationDbContext();
                var u = db.Users.Where(e => e.Email == applicationUser.Email).FirstOrDefault();
                if(u != null && u.Id != applicationUser.Id)
                {
                    ModelState.AddModelError("Email", "Email already exists");
                    return View(applicationUser);
                }
                try
                {
                    newContext.Entry(applicationUser).State = EntityState.Modified;
                    newContext.SaveChanges();
                }catch(Exception ec)
                {
                    ModelState.AddModelError("Username", "Username already exists");
                    return View(applicationUser);
                }
                if (currUser.Role == "Admin" && currUser.Id != applicationUser.Id)
                {
                    if (applicationUser.Role == "Lecturer") return RedirectToAction("Lecturers");
                    else if (applicationUser.Role == "Student") return RedirectToAction("Students");
                }
                return RedirectToAction("Index","Manage");
            }
            return View(applicationUser);
        }

        // GET: ApplicationUsers/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser applicationUser = db.Users.Find(id);
            if (applicationUser == null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            return View(applicationUser);
        }

        // POST: ApplicationUsers/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            ApplicationUser applicationUser = db.Users.Find(id);
            var courses = (from UserCourse in db.UserCourse where UserCourse.UserId == id select UserCourse.CourseId).ToArray();
            var userExams = db.UserExam.Where(e => e.UserId == id).ToArray();
            foreach(var ue in userExams)
            {
                db.UserExam.Remove(ue);
            }
            foreach(var cId in courses)
            {
                UserCourse uc = db.UserCourse.Find(id, cId);
                if (uc != null) db.UserCourse.Remove(uc);
            }
            db.Users.Remove(applicationUser);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Lecturer,Admin")]
        public ActionResult LecturerCourses(string id)
        {
            
            var userCourse = db.UserCourse.Where(e => e.UserId == id).ToArray();
            List<Course> courses = new List<Course>();
            foreach(var i in userCourse)
            {
                var course = db.Courses.Find(i.CourseId);
                courses.Add(course);
            }
            return PartialView(courses);
        }

        [HttpGet]
        public ActionResult CurrentUserCalendar()
        {
            return View();
        }
        public JsonResult GetCourses()
        {
            var currentUser = User.Identity.GetUserId();
            var userCourses = db.UserCourse.Where(e => e.UserId == currentUser).ToList();
            List<CourseViewModel> cm = new List<CourseViewModel>();
            foreach (var uc in userCourses)
            {
                var course = db.Courses.Find(uc.CourseId);
                courseService.getCourseViewModel(course, cm);
            }

            return new JsonResult { Data = cm, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }

        [HttpGet]
        public ActionResult ApplyMoedB(int CourseId, string UserId)
        {
            var exam = db.Exam.Where(e => e.CourseId == CourseId && e.Moed == "B").FirstOrDefault();
            var userExam = new UserExam { UserId = UserId, ExamId = exam.ExamId };
            db.UserExam.Add(userExam);
            db.SaveChanges();
            return RedirectToAction("MyExams", "Exams");
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
