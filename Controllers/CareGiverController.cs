using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EmployeeTest.Filters;
using EmployeeTest.Models;
using EmployeeTest.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EmployeeTest.Controllers
{
    [Authentication]
    public class CareGiverController : Controller
    {
        private readonly IOptions<Appsettings> _appSettings;
        private readonly DBContext _dbContext;
        private readonly IOptions<EmailSettings> _emailSettings;

        public CareGiverController(IOptions<Appsettings> appSettings, DBContext dbContext, IOptions<EmailSettings> emailSettings)
        {
            _appSettings = appSettings;
            _dbContext = dbContext;
            _emailSettings = emailSettings;
        }

        public IActionResult List()
        {
            var employees = new List<EmployeeViewModel>();
            try
            {
                //var totalQuestions = _dbContext.tbl_Questions.Where(w => w.IsActive == true).Count();
                var checkPercentage = _dbContext.tbl_TestPassingPercentages.Where(w => w.TestId == 1).Select(s => s.Value).FirstOrDefault();
                int iCheckPercentage = (checkPercentage == 0 ? (85) : Convert.ToInt32(checkPercentage));
                employees = (from emp in _dbContext.tbl_Attendants
                                 //join user_test in _dbContext.tbl_UserTests
                                 //on emp.UserId equals user_test.UserId
                                 //into user_test
                                 //from user_test1 in user_test.DefaultIfEmpty()
                                 //where (user_test1.Id > 0 ? user_test1.IsReset == false : true)
                             select new EmployeeViewModel
                             {
                                 EmployeeId = emp.Id,
                                 FirstName = emp.FirstName,
                                 MiddleName = emp.MiddleName,
                                 LastName = emp.LastName,
                                 Email = emp.Email,
                                 EmployeeNo = emp.EmployeeNo,
                                 UserId = emp.UserId ?? 0,

                             }).ToList();
                foreach (var employee in employees)
                {
                    employee.TestList = _dbContext.tbl_Tests.Select(s => new CareGiverTestViewModel
                    {
                        TestId = s.Id,
                        Name = s.Name
                    }).ToList();
                    int passesTest = 0;
                    if (employee.UserId > 0)
                    {

                        foreach (var item in employee.TestList)
                        {
                            var checkTest = _dbContext.tbl_UserTests.Where(w => w.UserId == employee.UserId && w.TestId == item.TestId).FirstOrDefault();
                            if (checkTest != null)
                            {
                                var totalQuestions = _dbContext.tbl_Questions.Where(w => w.IsActive == true && w.TestId == item.TestId).Count();
                                if (((int)Math.Round((double)(100 * checkTest.QuestionsRight.Value) / totalQuestions)) >= iCheckPercentage)
                                {
                                    passesTest++;
                                }


                            }

                        }
                        employee.PassedTest = passesTest;
                        employee.Totaltest = employee.TestList.Count();

                        var videoDuration = _dbContext.tbl_AttendentTestVideos.Where(w => w.UserId == employee.UserId).Sum(s => s.Duration);
                        employee.VideoDuration = videoDuration;


                    }

                }

            }
            catch (Exception ex)
            {

            }



            return View(employees);
        }

        public IActionResult Import()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Import(IFormFile file)
        {

            try
            {


                if (file == null || file.Length == 0)
                    return Content("file not selected");


                var attendantList = new List<Import_attendant>();
                using (var sreader = new StreamReader(file.OpenReadStream()))
                {
                    //First line is header. If header is not passed in csv then we can neglect the below line.
                    string[] headers = sreader.ReadLine().Split(',');
                    //Loop through the records
                    while (!sreader.EndOfStream)
                    {
                        string[] rows = sreader.ReadLine().Split(',');
                        string FirstName = "";
                        string MiddleName = "";
                        string LastName = "";
                        string Email = "";
                        string EmployeeNo = "";


                        FirstName = Convert.ToString(rows[0]);
                        MiddleName = (Convert.ToString(rows[1]));
                        LastName = Convert.ToString(rows[2]);
                        Email = (Convert.ToString(rows[3]));
                        EmployeeNo = (Convert.ToString(rows[4]));


                        attendantList.Add(new Import_attendant
                        {
                            FirstName = FirstName,
                            MiddleName = MiddleName,
                            LastName = LastName,
                            Email = Email,
                            EmployeeNo = EmployeeNo
                        });
                    }
                }

                _dbContext.tbl_Import_attendants.AddRange(attendantList);
                _dbContext.SaveChanges();

                DbfunctionUtility dbfunction = new DbfunctionUtility(_appSettings);
                DataSet ds = dbfunction.GetDataset("call import_employees();");
                ViewBag.imported = 1;


            }
            catch (Exception ex)
            {

                ViewBag.imported = 0;
            }
            return View();
        }

        public JsonResult SendEmail(string email)
        {
            EmailUtility _emaUtility = new EmailUtility(_appSettings, _emailSettings);
            string emailBody = @"<p>Please register to In-Service Compliance Application to start you test with following link</p>
                                    <br />
                                  <p><a href='" + _appSettings.Value.WebBaseURL + "/Account/Register'>" + _appSettings.Value.WebBaseURL + "/Register</a></p>";


            _emaUtility.SendEmail("", "Registration link for In-Service Compliance", emailBody, email.Split(","));
            return Json("Email sent successfully");
        }

        public JsonResult SendEmails()
        {
            EmailUtility _emaUtility = new EmailUtility(_appSettings, _emailSettings);
            var emails = (from emp in _dbContext.tbl_Attendants
                          where (emp.UserId ?? 0) == 0
                          select emp.Email).ToArray();

            string emailBody = @"<p>Please register to In-Service Compliance Application to start you test with following link</p>
                                    <br />
                                  <p><a href='" + _appSettings.Value.WebBaseURL + "/Account/Register'>" + _appSettings.Value.WebBaseURL + "/Register</a></p>";


            _emaUtility.SendEmail("", "Registration link for In-Service Compliance", emailBody, emails);
            return Json("Email sent successfully");
        }

        public IActionResult Reset(int UserTestId)
        {

            var checkUserTest = _dbContext.tbl_UserTests.Where(w => w.Id == UserTestId).FirstOrDefault();
            if (checkUserTest != null)
            {
                checkUserTest.IsReset = true;
                _dbContext.SaveChanges();

                UserTest userTest = new UserTest
                {
                    TestId = checkUserTest.TestId,
                    UserId = checkUserTest.UserId,
                    IsReset = false,
                    IsLocked = false,
                    QuestionsAttend = 0,
                    QuestionsRight = 0,
                    StartDate = DateTime.Now
                };

                _dbContext.tbl_UserTests.Add(userTest);
                _dbContext.SaveChanges();

            }

            return RedirectToAction("List", "CareGiver");

        }

        public IActionResult ViewAnswer(int userTestId)
        {

            TestUtility _testUtility = new TestUtility(_appSettings, _dbContext);
            var userAnswer = _testUtility.GetUserQuestionsWithAnswer(userTestId);

            return View("Answers", userAnswer);

        }

        public IActionResult ViewTest(int id)
        {

            TestUtility _testUtility = new TestUtility(_appSettings, _dbContext);
            var checkPercentage = _dbContext.tbl_TestPassingPercentages.Where(w => w.TestId == 1).Select(s => s.Value).FirstOrDefault();
            int iCheckPercentage = (checkPercentage == 0 ? (85) : Convert.ToInt32(checkPercentage));
           
            var employee = (from emp in _dbContext.tbl_Attendants
                            where emp.Id == id
                            select new EmployeeViewModel
                            {
                                EmployeeId = emp.Id,
                                FirstName = emp.FirstName,
                                MiddleName = emp.MiddleName,
                                LastName = emp.LastName,
                                Email = emp.Email,
                                EmployeeNo = emp.EmployeeNo,
                                UserId = emp.UserId ?? 0,

                            }).FirstOrDefault();

            employee.TestList = _dbContext.tbl_Tests.Select(s => new CareGiverTestViewModel
            {
                TestId = s.Id,
                Name = s.Name
            }).ToList();
            int passesTest = 0;

            if (employee?.UserId > 0)
            {

                
               

                foreach (var item in employee.TestList)
                {
                    var checkTest = _dbContext.tbl_UserTests.Where(w => w.UserId == employee.UserId && w.TestId == item.TestId).FirstOrDefault();
                    if (checkTest != null)
                    {
                        var totalQuestions = _dbContext.tbl_Questions.Where(w => w.IsActive == true && w.TestId == item.TestId).Count();
                        item.UserTestId = checkTest.Id;
                        item.TotalQuestion = totalQuestions;
                        item.Score = checkTest.QuestionsRight ?? 0;
                        item.IsLocked = checkTest.Id > 0 ? (checkTest.IsLocked.Value == true ? true : false) : false;
                        item.UserTestId = checkTest.Id;
                        item.QuestionsAttend = checkTest.QuestionsAttend ?? 0;
                        item.QuestionsRight = checkTest.QuestionsRight ?? 0;
                        item.PassingPercentage = (iCheckPercentage);
                        item.StartDate = checkTest.StartDate;
                        if (((int)Math.Round((double)(100 * checkTest.QuestionsRight.Value) / totalQuestions)) >= iCheckPercentage)
                        {
                            passesTest++;
                        }
                    }
                    else
                    {
                        item.UserTestId = 0;
                    }
                }

               

            }
            employee.PassedTest = passesTest;
            employee.Totaltest = employee.TestList.Count();

            return View("Tests", employee);

        }

        public JsonResult sendEmailLogin(string email)
        {
            EmailUtility _emaUtility = new EmailUtility(_appSettings, _emailSettings);
            string emailBody = @"<p>Please login toIn-Service Compliance Application to start you test with following link</p>
                                    <br />
                                  <p><a href='" + _appSettings.Value.WebBaseURL + "/Account/Login'>" + _appSettings.Value.WebBaseURL + "/Login</a></p>";


            _emaUtility.SendEmail("", "Registration link for In-Service Compliance", emailBody, email.Split(","));
            return Json("Email sent successfully");
        }

        public IActionResult GetCareGiverListByDate(DateTime startDate, DateTime endDate)
        {
            var checkPercentage = _dbContext.tbl_TestPassingPercentages.Where(w => w.TestId == 1).Select(s => s.Value).FirstOrDefault();
            int iCheckPercentage = (checkPercentage == 0 ? (85) : Convert.ToInt32(checkPercentage));


            var careGiverList = (from emp in _dbContext.tbl_Attendants
                                 select new EmployeeViewModel
                                 {
                                     EmployeeId = emp.Id,
                                     FirstName = emp.FirstName,
                                     MiddleName = emp.MiddleName,
                                     LastName = emp.LastName,
                                     Email = emp.Email,
                                     EmployeeNo = emp.EmployeeNo,
                                     UserId = emp.UserId ?? 0,

                                 }).ToList();

            foreach (var employee in careGiverList)
            {
                employee.TestList = (from test in _dbContext.tbl_Tests
                                     join user_test in _dbContext.tbl_UserTests
                                    on test.Id equals user_test.TestId
                                     where user_test.IsReset == false
                                     && user_test.EndDate >= startDate && user_test.EndDate <= endDate
                                     && user_test.UserId == employee.UserId
                                     select new CareGiverTestViewModel
                                     {
                                         TestId = test.Id,
                                         Name = test.Name,
                                         UserTestId = test.Id,
                                         Score = user_test.QuestionsRight ?? 0,
                                         IsLocked = test.Id > 0 ? (user_test.IsLocked.Value == true ? true : false) : false,
                                         QuestionsAttend = user_test.QuestionsAttend ?? 0,
                                         QuestionsRight = user_test.QuestionsRight ?? 0,
                                         PassingPercentage = (iCheckPercentage),
                                         StartDate = user_test.StartDate,

                                     }
                                  ).ToList();

                if (employee.UserId > 0)
                {



                    foreach (var item in employee.TestList)
                    {
                        var totalQuestions = _dbContext.tbl_Questions.Where(w => w.IsActive == true && w.TestId == item.TestId).Count();
                        item.TotalQuestion = totalQuestions;
                    }

                    var videoDuration = _dbContext.tbl_AttendentTestVideos.Where(w => w.UserId == employee.UserId).Sum(s => s.Duration);
                    employee.VideoDuration = videoDuration;


                }

            }



            return PartialView("_Table", careGiverList);
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

    }
}