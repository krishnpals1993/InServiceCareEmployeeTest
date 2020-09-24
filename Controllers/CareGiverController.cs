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
using AutoMapper;
using AutoMapper.QueryableExtensions;
using JqueryDataTables.ServerSide.AspNetCoreWeb.Infrastructure;
using JqueryDataTables.ServerSide.AspNetCoreWeb.Models;
using System.Text.Json;

namespace EmployeeTest.Controllers
{
    [Authentication]
    public class CareGiverController : Controller
    {
        private readonly IOptions<Appsettings> _appSettings;
        private readonly DBContext _dbContext;
        private readonly IOptions<EmailSettings> _emailSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfigurationProvider _mappingConfiguration;
        private ISession _session => _httpContextAccessor.HttpContext.Session;
        private string _rolename = "";
        private string _HrGroupId = "";
        private int UserId = 0;
        public CareGiverController(IOptions<Appsettings> appSettings, DBContext dbContext, IOptions<EmailSettings> emailSettings, IHttpContextAccessor HttpContextAccessor, IConfigurationProvider mappingConfiguration)
        {
            _appSettings = appSettings;
            _dbContext = dbContext;
            _emailSettings = emailSettings;
            _httpContextAccessor = HttpContextAccessor;
            _mappingConfiguration = mappingConfiguration;
            _rolename = _session.GetString("RoleName");
            _HrGroupId = _session.GetString("HrGroupId");
            int.TryParse(_session.GetString("UserId"), out UserId);
        }

        public IActionResult List()
        {
            return View(new EmployeeViewModel_datatable());
        }

        [HttpPost]
        public async Task<IActionResult> LoadTable([FromBody]JqueryDataTablesParameters param)
        {
            try
            {
                HttpContext.Session.SetString(nameof(JqueryDataTablesParameters), JsonSerializer.Serialize(param));
                IQueryable<EmployeeViewModel_datatable> employees;
                string queryDef = "call get_careGiverList()";
                DbfunctionUtility dbfunction = new DbfunctionUtility(_appSettings);
                CommanUtility _commanUtility = new CommanUtility(_appSettings);
                DateTime startDate = new DateTime(); DateTime endDate = new DateTime(); int i = 0; bool isDateFilter = false;
                DataSet ds = new DataSet();

                foreach (var column in param.AdditionalValues)
                {
                    if (Convert.ToString(column) != "")
                    {
                        isDateFilter = true;
                        var dateParts = column.Split("/");

                        // var date = dateParts[1] + "/" + dateParts[0] + "/" + dateParts[2];
                        if (i == 0)
                        {


                            DateTime.TryParse(column, out startDate);
                        }
                        else if (i == 1)
                        {
                            DateTime.TryParse(column, out endDate);
                        }

                    }
                    i++;
                }

                if (isDateFilter == false)
                {
                    ds = dbfunction.GetDataset(@"call get_careGiverList()");
                }
                else
                {
                    ds = dbfunction.GetDataset(@"call get_careGiverListByDate('" + startDate.ToString("yyyy/MM/dd") + "','" + endDate.ToString("yyyy/MM/dd") + "') ");
                }

                employees = (from row in ds.Tables[0].AsEnumerable()
                             select new EmployeeViewModel_datatable
                             {
                                 EmployeeId = Convert.ToInt32(row["Id"]),
                                 FirstName = Convert.ToString(row["FirstName"]),
                                 MiddleName = Convert.ToString(row["MiddleName"]),
                                 LastName = Convert.ToString(row["LastName"]),
                                 Email = Convert.ToString(row["Email"]),
                                 EmployeeNo = Convert.ToString(row["EmployeeNo"]),
                                 UserId = Convert.ToInt32(row["UserId"]),
                                 HrGroupName = Convert.ToString(row["HrGroup"]),
                                 HrGroupId = Convert.ToInt32(row["HrGroupId"]),
                                 PassedTest = Convert.ToInt32(row["PassedTest"]),
                                 Totaltest = Convert.ToInt32(row["total_tests_1"]),
                                 VideoDuration = Convert.ToString(row["VideoDuration"]) == "" ? (Decimal?)null : Convert.ToDecimal(row["VideoDuration"]),
                                 ExamDate = Convert.ToString(row["ExamDate"]) == "" ? (DateTime?)null : Convert.ToDateTime(row["ExamDate"]),
                                 ValidEmail = _commanUtility.CheckValidEmail(Convert.ToString(row["Email"]))

                             }).AsQueryable();

                if (_rolename.ToLower() == "hr")
                {
                    employees = employees.Where(w => _HrGroupId.Contains("/" + w.HrGroupId.ToString() + "/")).AsQueryable();
                }


                employees = SearchOptionsProcessor<EmployeeViewModel_datatable, EmployeeViewModel_datatable>.Apply(employees, param.Columns);
                employees = SortOptionsProcessor<EmployeeViewModel_datatable, EmployeeViewModel_datatable>.Apply(employees, param);

                var size = employees.Count();


                if (Convert.ToString(param.Search?.Value) != "")
                {
                    var serchValue = param.Search?.Value.ToLower();
                    employees = employees.Where(w =>
                                  (w.FirstName.ToLower().Contains(serchValue) ? true :
                                  (w.MiddleName.ToLower().Contains(serchValue) ? true :
                                  ((w.LastName.ToLower().Contains(serchValue) ? true :
                                  ((w.EmployeeNo.ToLower().Contains(serchValue) ? true :
                                  ((w.Email.ToLower().Contains(serchValue) ? true :
                                   ((w.HrGroupName.ToLower().Contains(serchValue) ? true : false))))))))
                                )));

                }

                var items = employees
                   .Skip((param.Start / param.Length) * param.Length)
                   .Take(param.Length)
                   .ProjectTo<EmployeeViewModel_datatable>(_mappingConfiguration)
                   .ToArray();


                var result = new JqueryDataTablesPagedResults<EmployeeViewModel_datatable>
                {
                    Items = items,
                    TotalSize = size
                };

                return new JsonResult(new JqueryDataTablesResult<EmployeeViewModel_datatable>
                {
                    Draw = param.Draw,
                    Data = result.Items,
                    RecordsFiltered = result.TotalSize,
                    RecordsTotal = result.TotalSize
                });
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return new JsonResult(new { error = "Internal Server Error" });
            }
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
                        string HrGroup = "";


                        FirstName = Convert.ToString(rows[0]);
                        MiddleName = (Convert.ToString(rows[1]));
                        LastName = Convert.ToString(rows[2]);
                        Email = (Convert.ToString(rows[3]));
                        EmployeeNo = (Convert.ToString(rows[4]));
                        HrGroup = (Convert.ToString(rows[5]));


                        attendantList.Add(new Import_attendant
                        {
                            FirstName = FirstName,
                            MiddleName = MiddleName,
                            LastName = LastName,
                            Email = Email,
                            EmployeeNo = EmployeeNo,
                            HrGroup = HrGroup

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
                    var checkTest = _dbContext.tbl_UserTests.Where(w => w.UserId == employee.UserId && w.TestId == item.TestId && w.IsReset == false).FirstOrDefault();
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
            var careGiverList = new List<EmployeeViewModel>();
            try
            {
                DbfunctionUtility dbfunction = new DbfunctionUtility(_appSettings);
                DataSet ds = dbfunction.GetDataset(@"call get_careGiverListByDate('" + startDate.ToString("yyyy/MM/dd") + "','" + endDate.ToString("yyyy/MM/dd") + "') ");
                careGiverList = (from row in ds.Tables[0].AsEnumerable()
                                 select new EmployeeViewModel
                                 {
                                     EmployeeId = Convert.ToInt32(row["Id"]),
                                     FirstName = Convert.ToString(row["FirstName"]),
                                     MiddleName = Convert.ToString(row["MiddleName"]),
                                     LastName = Convert.ToString(row["LastName"]),
                                     Email = Convert.ToString(row["Email"]),
                                     EmployeeNo = Convert.ToString(row["EmployeeNo"]),
                                     UserId = Convert.ToInt32(row["UserId"]),
                                     HrGroupName = Convert.ToString(row["HrGroup"]),
                                     HrGroupId = Convert.ToInt32(row["HrGroupId"]),
                                     PassedTest = Convert.ToInt32(row["PassedTest"]),
                                     Totaltest = Convert.ToInt32(row["total_tests_1"]),
                                     VideoDuration = Convert.ToString(row["VideoDuration"]) == "" ? (Decimal?)null : Convert.ToDecimal(row["VideoDuration"]),
                                     ExamDate = Convert.ToString(row["ExamDate"]) == "" ? (DateTime?)null : Convert.ToDateTime(row["ExamDate"])
                                 }).ToList();
            }
            catch (Exception ex)
            {


            }


            if (_rolename.ToLower() == "hr")
            {
               // careGiverList = careGiverList.Where(w => w.HrGroupId == _HrGroupId).ToList();
            }

            return PartialView("_Table", careGiverList);
        }

        public ActionResult Add()
        {
            CareGiverViewModel careGiver = new CareGiverViewModel();
            careGiver.HrGroupList = _dbContext.tbl_HrGroups.Where(w => w.IsActive == true)
              .Select(s => new HrGroupViewModel
              {
                  Id = s.Id,
                  Name = s.Name

              }).ToList();
            return View(careGiver);
        }

        [HttpPost]
        public ActionResult Add(CareGiverViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.Email = model.Email.Trim();
                    var checkAttendent = _dbContext.tbl_Attendants.Where(w => w.Email == model.Email).FirstOrDefault();
                    if (checkAttendent == null)
                    {
                        checkAttendent = _dbContext.tbl_Attendants.Where(w => w.EmployeeNo == model.EmployeeNo).FirstOrDefault();
                        if (checkAttendent == null)
                        {
                            Attendant attendant = new Attendant()
                            {
                                FirstName = model.FirstName,
                                MiddleName = model.MiddleName,
                                LastName = model.LastName,
                                EmployeeNo = model.EmployeeNo,
                                Email = model.Email,
                                CreatedDate = DateTime.Now,
                                CreatedBy = UserId,
                                HrGroupId = model.HrGroupId

                            };

                            _dbContext.tbl_Attendants.Add(attendant);
                            _dbContext.SaveChanges();

                            ViewBag.SuccessMessage = "Care Giver added successfully";

                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Employee no is already exists";

                        }






                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Email is already exists";

                    }
                }
            }
            catch (Exception ex)
            {
            }

            model.HrGroupList = _dbContext.tbl_HrGroups.Where(w => w.IsActive == true)
                  .Select(s => new HrGroupViewModel
                  {
                      Id = s.Id,
                      Name = s.Name

                  }).ToList();

            return View(model);
        }


        public ActionResult Edit(int id)
        {
            CareGiverViewModel model = new CareGiverViewModel();
            var checkCareGiver = _dbContext.tbl_Attendants.Where(w => w.Id == id).FirstOrDefault();

            model.HrGroupList = _dbContext.tbl_HrGroups.Where(w => w.IsActive == true)
              .Select(s => new HrGroupViewModel
              {
                  Id = s.Id,
                  Name = s.Name

              }).ToList();

            model.FirstName = checkCareGiver.FirstName;
            model.LastName = checkCareGiver.LastName;
            model.MiddleName = checkCareGiver.MiddleName;
            model.Email = checkCareGiver.Email;
            model.HrGroupId = checkCareGiver.HrGroupId ?? 0;
            model.Id = checkCareGiver.Id;
            model.EmployeeNo = checkCareGiver.EmployeeNo;

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(CareGiverViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.Email = model.Email.Trim();
                    var checkAttendent = _dbContext.tbl_Attendants.Where(w => w.Email == model.Email && w.Id != model.Id).FirstOrDefault();
                    if (checkAttendent == null)
                    {
                        checkAttendent = _dbContext.tbl_Attendants.Where(w => w.EmployeeNo == model.EmployeeNo && w.Id != model.Id).FirstOrDefault();
                        if (checkAttendent == null)
                        {

                            checkAttendent = _dbContext.tbl_Attendants.Where(w => w.Id == model.Id).FirstOrDefault();

                            checkAttendent.FirstName = model.FirstName;
                            checkAttendent.LastName = model.LastName;
                            checkAttendent.MiddleName = model.MiddleName;
                            checkAttendent.Email = model.Email;
                            checkAttendent.HrGroupId = model.HrGroupId;
                            checkAttendent.EmployeeNo = model.EmployeeNo;

                            _dbContext.SaveChanges();

                            ViewBag.SuccessMessage = "Care Giver updated successfully";

                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Employee no is already exists";

                        }

                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Email is already exists";

                    }
                }
            }
            catch (Exception ex)
            {
            }

            model.HrGroupList = _dbContext.tbl_HrGroups.Where(w => w.IsActive == true)
                  .Select(s => new HrGroupViewModel
                  {
                      Id = s.Id,
                      Name = s.Name

                  }).ToList();

            return View(model);
        }


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

    }
}