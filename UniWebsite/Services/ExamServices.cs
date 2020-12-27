using SCE___FINAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SCE___FINAL.ViewModels;

namespace SCE___FINAL.Services
{
    public class ExamServices
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        //The date of Moed B is after Moed A
        public bool validateMoedBDate(DateTime MoedB, int CourseId)
        {
            var exam = db.Exam.Where(e => e.CourseId == CourseId && e.Moed == "A").FirstOrDefault();
            if (exam.Date >= MoedB) return false;
            return true;

        }
        //The date of the exam is after the end date of the course
        public bool validateDate(DateTime date, int CourseId)
        {
            var course = db.Courses.Find(CourseId);
            if (date < course.FinishDate) return false;
            return true;
        }
        //Connect the users to the exam
        public void createUserExam(string user, Exam exam)
        {
            var examUser = new UserExam
            {
                UserId = user,
                ExamId = exam.ExamId
            };
            db.UserExam.Add(examUser);
            
        }

        public void assignUsersToExam(Exam exam)
        {
            var users = db.UserCourse.Where(e => e.CourseId == exam.CourseId).ToArray();
            bool flag = false;
            if (users != null)
            {
                if (exam.Moed == "B")
                {
                    flag = true;
                }
                foreach (var user in users)
                {
                    var lecturer = db.Users.Where(e => e.Id == user.UserId && e.Role == "Lecturer").FirstOrDefault();
                    if (flag && lecturer != null)
                    {
                        createUserExam(lecturer.Id, exam);
                        break;
                    }
                    else
                    {
                        createUserExam(user.UserId, exam);
                    }

                }db.SaveChanges();

            }
            
        }


        internal void getExamViewModel(Exam exam, List<ExamViewModel> examViewModel)
        {
            var course = db.Courses.Find(exam.CourseId);
            var evm = new ExamViewModel
            {
                course = course.Name,
                Hours = exam.Hours,
                Moed = exam.Moed,
                StartDate = exam.Date,
                EndDate = exam.Date.AddHours(exam.Hours),
                Class = exam.Class
            };
            examViewModel.Add(evm);
        }
        internal void AssignLecturerToExams(int CourseId,string UserId)
        {
            var exams = db.Exam.Where(e => e.CourseId == CourseId).ToArray();
            if (exams != null)
            {
                foreach (var exam in exams)
                {
                    var userExam = new UserExam
                    {
                        ExamId = exam.ExamId,
                        UserId = UserId
                    };
                    db.UserExam.Add(userExam);
                }
            }
        }
    }
}