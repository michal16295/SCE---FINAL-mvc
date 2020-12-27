using SCE___FINAL.Models;
using SCE___FINAL.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SCE___FINAL.Services
{
    public class CourseServices
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ExamServices examService = new ExamServices();

        public bool isClassFree(string Class, DateTime StartDate, int? Hours)
        {
            //get all the courses
            var courses = db.Courses.ToArray();
            var exams = db.Exam.ToArray();
            bool res = false;
            foreach (var course in courses)
            {
                if (course.Class == Class)
                {
                    res =  isDateMatches(StartDate, course.StartDate, course.FinishDate, Hours);
                    if (res) return false;
                }
                
            }
            foreach (var exam in exams)
            {
                
                if (exam.Class == Class)
                {
                    res = isDateMatches(StartDate, exam.Date, exam.Date.AddHours(exam.Hours), Hours);
                    if (res) return false;
                }

            }
            return true;
        }
        //False if the dates dont match
        public bool isDateMatches(DateTime StartDate, DateTime cStartData, DateTime cFinishDate, int? Hours)
        {
            if (StartDate > cFinishDate.AddHours(Convert.ToDouble(Hours))) return false;
            var dayOfTheWeak = cStartData.DayOfWeek;
            var inputDay = StartDate.DayOfWeek;
            if (inputDay == dayOfTheWeak)
            {
                if (StartDate.Hour >= cStartData.Hour + Hours) return false;
                if (StartDate.Hour + Hours < cStartData.Hour) return false;
                return true;
            }
            return false;
        }

        public void getCourseViewModel(Course course, List<CourseViewModel> cm)
        {
            int weeks = Convert.ToInt32((course.FinishDate - course.StartDate).TotalDays / 7);
            var WEEK = 7;
            for (var i = 0; i <= weeks; i++)
            {
                var c = new CourseViewModel
                {
                    Name = course.Name,
                    Class = course.Class,
                    Hours = course.Hours,
                    StartDate = course.StartDate.AddDays(WEEK * i),
                    FinishDate = course.StartDate.AddDays(WEEK * i).AddHours(Convert.ToInt32(course.Hours)),
                    Courseid = course.Courseid,
                    Lecturer = getLecturer(course.Courseid),

                };
                cm.Add(c);
            }

        }
        public string getLecturer(int courseid)
        {
            var usersIds = db.UserCourse.Where(uc => uc.CourseId == courseid).ToList();
            var lecturer = "";
            foreach (var id in usersIds)
            {
                ApplicationUser user = db.Users.Where(u => u.Id == id.UserId && u.Role == "Lecturer").FirstOrDefault();
                if (user != null)
                {
                    lecturer = user.FirstName + " " + user.LastName;
                    break;
                }
                

            }
            return lecturer;
        }
        // if true: the lecturer has classes at the same time
        public bool checkUsersHours(Course course, string UsersId)
        {

            var courses = (from UserCourse in db.UserCourse where UserCourse.UserId == UsersId select UserCourse.CourseId).ToArray();
            bool res = false;
            foreach (var _id in courses)
            {
                //Find all the User courses
                Course co = db.Courses.Find(_id);
                res = isDateMatches(course.StartDate, co.StartDate, co.FinishDate, course.Hours);
                if (res) break;
            }
            return res;
        }
        public void deleteCourse(int id)
        {
            Course course = db.Courses.Find(id);
            var exam = db.Exam.Where(e => e.CourseId == id).ToArray();
            foreach(var e in exam)
            {
                var userExams = db.UserExam.Where(ue => ue.ExamId == e.ExamId).ToArray();
                foreach (var ue in userExams)
                {
                    db.UserExam.Remove(ue);
                }
                db.Exam.Remove(e);
            }
            var users = (from UserCourse in db.UserCourse where UserCourse.CourseId == id select UserCourse.UserId).ToArray();

            foreach (var _id in users)
            {
                UserCourse uc = db.UserCourse.Find(_id, id);
                db.UserCourse.Remove(uc);
            }
            db.Courses.Remove(course);
            db.SaveChanges();
        }
        internal CourseViewModel GetCourseDetails(int? id, Course course, string lecturer)
        {
            var MoedA = db.Exam.Where(t => t.CourseId == id && t.Moed == "A").FirstOrDefault();
            var MoedB = db.Exam.Where(t => t.CourseId == id && t.Moed == "B").FirstOrDefault();
            var vm = new CourseViewModel
            {
                Lecturer = lecturer,
                Name = course.Name,
                Hours = course.Hours,
                Class = course.Class,
                StartDate = course.StartDate,
                FinishDate = course.FinishDate,
                Courseid = course.Courseid,
            };

            if (MoedA != null) vm.MoedA = MoedA.Date;
            if (MoedB != null) vm.MoedB = MoedB.Date;
            return vm;
        }

        internal List<StudentViewModel> StudentsList(int id)
        {
            var userCourses = db.UserCourse.Where(e => e.CourseId == id).ToList();
            var exams = db.Exam.Where(e => e.CourseId == id).ToArray();
            List<StudentViewModel> users = new List<StudentViewModel>();
            foreach (var userCourse in userCourses)
            {

                var user = db.Users.Where(u => u.Role == "Student" && u.Id == userCourse.UserId).FirstOrDefault();
                if (user != null)
                {
                    var studentViewModel = new StudentViewModel
                    {
                        CourseId = id,
                        id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        UserName = user.UserName,
                        Email = user.Email
                    };
                    if (userCourse != null) studentViewModel.FinalGrade = userCourse.Grade;
                    foreach (var exam in exams)
                    {
                        var userExam = db.UserExam.Find(user.Id, exam.ExamId);

                        if (userExam != null && exam.Moed == "A")
                        {
                            studentViewModel.MoedAGrade = userExam.Grade;
                            
                        }
                        if (userExam != null && exam.Moed == "B")
                        {
                            studentViewModel.MoedBGrade = userExam.Grade;
                            
                        }
                    }
                    users.Add(studentViewModel);
                }

            }return users;
        }
        public ApplicationUser getLecturerExamMoedB(int courseid)
        {
            var usersIds = db.UserCourse.Where(uc => uc.CourseId == courseid).ToList();
            foreach (var id in usersIds)
            {
                ApplicationUser user = db.Users.Where(u => u.Id == id.UserId && u.Role == "Lecturer").FirstOrDefault();
                if (user != null)
                {
                    return user;
                }

            }
            return null;
        }



    }
}