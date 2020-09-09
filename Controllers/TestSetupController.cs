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
    public class TestSetupController : Controller
    {
        private readonly IOptions<Appsettings> _appSettings;
        private readonly DBContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession _session => _httpContextAccessor.HttpContext.Session;
        private int UserId = 0;

        public TestSetupController(IOptions<Appsettings> appSettings, DBContext dbContext, IHttpContextAccessor HttpContextAccessor)
        {
            _appSettings = appSettings;
            _dbContext = dbContext;
            _httpContextAccessor = HttpContextAccessor;
            int.TryParse(_session.GetString("UserId"), out UserId);
        }

        public IActionResult List()
        {
            List<QuestionViewModel> questions = new List<QuestionViewModel>();
            try
            {
                questions = (from s in _dbContext.tbl_Questions
                             join test in _dbContext.tbl_Tests on s.TestId equals test.Id
                             into test
                             from test1 in test.DefaultIfEmpty()
                             join season in _dbContext.tbl_Seasons
                             on test1.SeasonId equals season.Id
                             into season
                             from season1 in season.DefaultIfEmpty()
                             select new QuestionViewModel
                             {
                                 Question = s.Question,
                                 Choice1 = s.Choice1,
                                 Choice2 = s.Choice2,
                                 Choice3 = s.Choice3,
                                 Choice4 = s.Choice4,
                                 Choice5 = s.Choice5,
                                 Answer = s.Answer,
                                 QuestionId = s.Id,
                                 TestName = test1.Name,
                                 SeasonName = season1.Name

                             }).ToList();
            }
            catch (Exception ex)
            {
            }


            return View(questions);
        }

        public ActionResult Add()
        {
            QuestionViewModel model = new QuestionViewModel();
            model.TestList = (from test in _dbContext.tbl_Tests
                              join season in _dbContext.tbl_Seasons
                              on test.SeasonId equals season.Id
                              where test.IsActive??true
                              select new TestViewModel
                              {
                                  Name = test.Name + " (" + season.Name + ")",
                                  Id = test.Id,
                                  IsActive = test.IsActive
                              }).ToList();

            return View(model);
        }

        public ActionResult Edit(string id)
        {
            QuestionViewModel model = new QuestionViewModel();
            try
            {

                {
                    DbfunctionUtility dbfunction = new DbfunctionUtility(_appSettings);
                    DataSet ds = dbfunction.GetDataset("select * from questions where Id= '" + id + "'");
                    int idInt = 0;
                    int.TryParse(id, out idInt);
                    model = _dbContext.tbl_Questions.Where(w => w.Id == idInt)
                        .Select(s => new QuestionViewModel
                        {
                            Question = s.Question,
                            Choice1 = s.Choice1,
                            Choice2 = s.Choice2,
                            Choice3 = s.Choice3,
                            Choice4 = s.Choice4,
                            Choice5 = s.Choice5,
                            Answer = s.Answer,
                            QuestionId = s.Id,
                            TestId = s.TestId ?? 0

                        }).FirstOrDefault();

                    model.TestList = (from test in _dbContext.tbl_Tests
                                      join season in _dbContext.tbl_Seasons
                                      on test.SeasonId equals season.Id
                                      select new TestViewModel
                                      {
                                          Name = test.Name + " (" + season.Name + ")",
                                          Id = test.Id,
                                          IsActive = test.IsActive
                                      }).ToList();

                    if (model != null)
                    {
                        return View(model);
                    }
                    else
                    {
                        return RedirectToAction("List", "Question");
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return View(model);

        }

        [HttpPost]
        public ActionResult Add(QuestionViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var checkQuestion = _dbContext.tbl_Questions.Where(w => w.TestId == model.TestId && w.Question == model.Question).FirstOrDefault();
                    if (checkQuestion == null)
                    {
                        if (Convert.ToString(model.Choice3) == "")
                        {
                            if (Convert.ToString(model.Choice4) != "")
                            {
                                model.Choice3 = model.Choice4;
                                model.Choice4 = "";
                            }
                            else if (Convert.ToString(model.Choice5) != "")
                            {
                                model.Choice3 = model.Choice5;
                                model.Choice5 = "";
                            }
                        }

                        if (Convert.ToString(model.Choice4) == "")
                        {
                            if (Convert.ToString(model.Choice5) != "")
                            {
                                model.Choice4 = model.Choice5;
                                model.Choice5 = "";
                            }
                        }

                        Questions question = new Questions()
                        {
                            Question = model.Question,
                            Choice1 = model.Choice1,
                            Choice2 = model.Choice2,
                            Choice3 = model.Choice3,
                            Choice4 = model.Choice4,
                            Choice5 = model.Choice5,
                            Answer = model.Answer,
                            IsActive = true,
                            CreatedBy = UserId,
                            CreatedDate = DateTime.Now,
                            TestId = model.TestId
                        };

                        _dbContext.tbl_Questions.Add(question);
                        _dbContext.SaveChanges();

                        ViewBag.SuccessMessage = "Question added successfully";
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Question is already exists";
                        model.TestList = (from test in _dbContext.tbl_Tests
                                          join season in _dbContext.tbl_Seasons
                                          on test.SeasonId equals season.Id
                                          select new TestViewModel
                                          {
                                              Name = test.Name + " (" + season.Name + ")",
                                              Id = test.Id,
                                              IsActive = test.IsActive
                                          }).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(QuestionViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    DbfunctionUtility dbfunction = new DbfunctionUtility(_appSettings);
                    var checkQuestion = _dbContext.tbl_Questions.Where(w => w.TestId == model.TestId && w.Question == model.Question && w.Id != model.QuestionId).FirstOrDefault();
                    if (checkQuestion == null)
                    {
                        if (Convert.ToString(model.Choice3) == "")
                        {
                            if (Convert.ToString(model.Choice4) != "")
                            {
                                model.Choice3 = model.Choice4;
                                model.Choice4 = "";
                            }
                            else if (Convert.ToString(model.Choice5) != "")
                            {
                                model.Choice3 = model.Choice5;
                                model.Choice5 = "";
                            }
                        }

                        if (Convert.ToString(model.Choice4) == "")
                        {
                            if (Convert.ToString(model.Choice5) != "")
                            {
                                model.Choice4 = model.Choice5;
                                model.Choice5 = "";
                            }
                        }

                        var question = _dbContext.tbl_Questions.Where(w => w.Id == model.QuestionId).FirstOrDefault();
                        question.Question = model.Question;
                        question.Choice1 = model.Choice1;
                        question.Choice2 = model.Choice2;
                        question.Choice3 = model.Choice3;
                        question.Choice4 = model.Choice4;
                        question.Choice5 = model.Choice5;
                        question.TestId = model.TestId;
                        question.Answer = model.Answer;
                        question.ModifiedBy = UserId;
                        question.ModifiedDate = DateTime.Now;
                        _dbContext.SaveChanges();


                        ViewBag.SuccessMessage = "Question updated successfully";
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Question is already exists";
                        model.TestList = (from test in _dbContext.tbl_Tests
                                          join season in _dbContext.tbl_Seasons
                                          on test.SeasonId equals season.Id
                                          select new TestViewModel
                                          {
                                              Name = test.Name + " (" + season.Name + ")",
                                              Id = test.Id,
                                              IsActive = test.IsActive
                                          }).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return View(model);
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                QuestionViewModel model = new QuestionViewModel();
                DbfunctionUtility dbfunction = new DbfunctionUtility(_appSettings);
                var query = " delete from questions  where Id ='" + id + "' ;";
                DataSet ds = dbfunction.GetDataset(query);
                response.Status = "1";
                response.Message = "Question deleted successfully";
            }
            catch (Exception ex)
            {
            }

            return Json(response);
        }

        public IActionResult PassingPercentage()
        {
            var checkPercentage = _dbContext.tbl_TestPassingPercentages.Where(w => w.TestId == 1).
                Select(s => new TestPassingPercentageViewModel
                {
                    Value = s.Value
                }).FirstOrDefault();
            checkPercentage = checkPercentage ?? new TestPassingPercentageViewModel { Value = 0 };
            return View(checkPercentage);
        }

        [HttpPost]
        public IActionResult PassingPercentage(TestPassingPercentageViewModel model)
        {
            var checkPercentage = _dbContext.tbl_TestPassingPercentages.Where(w => w.TestId == 1).
                Select(s => new TestPassingPercentageViewModel
                {
                    Value = s.Value
                }).FirstOrDefault();

            if (checkPercentage == null)
            {
                TestPassingPercentage passingPercentage = new TestPassingPercentage()
                {
                    Value = model.Value,
                    IsActive = true,
                    CreatedBy = UserId,
                    CreatedDate = DateTime.Now,
                    TestId = 1

                };
                _dbContext.tbl_TestPassingPercentages.Add(passingPercentage);
                _dbContext.SaveChanges();
            }
            else
            {
                var passPercentage = _dbContext.tbl_TestPassingPercentages.Where(w => w.TestId == 1).FirstOrDefault();
                passPercentage.Value = model.Value;
                passPercentage.ModifiedBy = UserId;
                passPercentage.ModifiedDate = DateTime.Now;
                _dbContext.SaveChanges();
            }

            ViewBag.SuccessMessage = "Percentage added successfully";
            return View(checkPercentage);
        }


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}