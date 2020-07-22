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
    public class TestController : Controller
    {
        private readonly IOptions<Appsettings> _appSettings;
        private readonly DBContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession _session => _httpContextAccessor.HttpContext.Session;
        private int UserId = 0;

        public TestController(IOptions<Appsettings> appSettings, DBContext dbContext, IHttpContextAccessor HttpContextAccessor)
        {
            _appSettings = appSettings;
            _dbContext = dbContext;
            _httpContextAccessor = HttpContextAccessor;
            int.TryParse(_session.GetString("UserId"), out UserId);
        }

        public IActionResult Question(int id,int TestId)
        {
            TestQuestionViewModel model = new TestQuestionViewModel();
            DbfunctionUtility dbfunction = new DbfunctionUtility(_appSettings);
            TestUtility _testUtility = new TestUtility(_appSettings, _dbContext);
            var checkUserTest = _dbContext.tbl_UserTests.Where(w => w.UserId == UserId && w.TestId == TestId && w.IsReset == false).FirstOrDefault();
            if (checkUserTest == null)
            {
                var sNo = 0;
                var questionList = _dbContext.tbl_Questions.Select(s => new TestQuestionViewModel
                {
                    QuestionId = s.Id,
                    Question = s.Question,
                    Choice1 = s.Choice1,
                    Choice2 = s.Choice2,
                    Choice3 = s.Choice3,
                    Choice4 = s.Choice4,
                    Choice5 = s.Choice5,
                    Answer = s.Answer,
                    

                }).ToList();

                foreach (var item in questionList)
                {
                    item.SeqNo = ++sNo;
                }
                model = questionList.FirstOrDefault();
                model.SeqNo = 1;
                model.TestId = TestId;
            }
            else
            {
                var questionList = _testUtility.GetUserQuestions(UserId,TestId);
                model = questionList.Where(w => w.SeqNo == id).FirstOrDefault();
                model.TestId =TestId;
                model.MaxSequenceId = questionList.LastOrDefault().SeqNo;
                model.MinSequenceId = questionList.FirstOrDefault().SeqNo;

            }

            model.TestName = _dbContext.tbl_Tests.Where(w=>w.Id == TestId).Select(s=>s.Name).FirstOrDefault();

            return View(model);
        }

        [HttpPost]
        public IActionResult Question(TestQuestionViewModel model, string submit)
        {
            int TestId = model.TestId;
            DbfunctionUtility dbfunction = new DbfunctionUtility(_appSettings);
            TestUtility _testUtility = new TestUtility(_appSettings, _dbContext);
            var checkUserTest = _dbContext.tbl_UserTests.Where(w => w.UserId == UserId && w.TestId == TestId  && w.IsReset == false).FirstOrDefault();
            if (submit == "Save")
            {

                var checkAnswer = _dbContext.tbl_UserTestAnswers.Where(w => w.UserTestId == checkUserTest.Id && w.QuestionId == model.QuestionId).FirstOrDefault();
                if (checkAnswer == null)
                {
                    UserTestAnswer userTestAnswer = new UserTestAnswer()
                    {
                        QuestionId = model.QuestionId,
                        UserTestId = checkUserTest.Id,
                        UserSelected = model.UserAnswer,
                        IsRight = (model.Answer == model.UserAnswer ? true : false)
                    };

                    _dbContext.tbl_UserTestAnswers.Add(userTestAnswer);
                    _dbContext.SaveChanges();

                    checkUserTest.QuestionsAttend = checkUserTest.QuestionsAttend + 1;
                    checkUserTest.QuestionsRight = checkUserTest.QuestionsRight + (userTestAnswer.IsRight ? 1 : 0);
                    _dbContext.SaveChanges();
                }
                else
                {


                    if (checkAnswer.IsRight == true && (model.Answer != model.UserAnswer))
                    {
                        checkUserTest.QuestionsRight = checkUserTest.QuestionsRight - 1;
                    }
                    else if (checkAnswer.IsRight == false && (model.Answer == model.UserAnswer))
                    {
                        checkUserTest.QuestionsRight = checkUserTest.QuestionsRight + 1;
                    }

                    _dbContext.SaveChanges();

                    checkAnswer.UserSelected = model.UserAnswer;
                    checkAnswer.IsRight = (model.Answer == model.UserAnswer ? true : false);
                    _dbContext.SaveChanges();

                }

                var questionList = _testUtility.GetUserQuestions(UserId,TestId);

                if (model.SeqNo != model.MaxSequenceId)
                {
                    model = questionList.Where(w => w.SeqNo == model.SeqNo + 1).FirstOrDefault();
                    model.MaxSequenceId = questionList.LastOrDefault().SeqNo;
                    model.MinSequenceId = questionList.FirstOrDefault().SeqNo;


                }
            }
            if (submit == "Previous")
            {

                var questionList = _testUtility.GetUserQuestions(UserId,TestId);
                model = questionList.Where(w => w.SeqNo == model.SeqNo - 1).FirstOrDefault();
                model.MaxSequenceId = questionList.LastOrDefault().SeqNo;
                model.MinSequenceId = questionList.FirstOrDefault().SeqNo;


            }
            if (submit == "Next")
            {

                var questionList = _testUtility.GetUserQuestions(UserId,TestId);
                model = questionList.Where(w => w.SeqNo == model.SeqNo + 1).FirstOrDefault();
                model.MaxSequenceId = questionList.LastOrDefault().SeqNo;
                model.MinSequenceId = questionList.FirstOrDefault().SeqNo;


            }
            if (submit == "Finish")
            {

                checkUserTest.IsLocked = true;
                checkUserTest.EndDate = DateTime.Now;
                _dbContext.SaveChanges();
                return RedirectToAction("Exam", "Attendant");
            }

            return RedirectToAction("Question", new { id = model.SeqNo, TestId = TestId });

        }

        public IActionResult Start(int TestId)
        {
            UserTest userTest = new UserTest
            {
                TestId = TestId,
                UserId = UserId,
                IsReset = false,
                IsLocked = false,
                QuestionsAttend = 0,
                QuestionsRight = 0,
                StartDate = DateTime.Now
            };

            _dbContext.tbl_UserTests.Add(userTest);
            _dbContext.SaveChanges();

            return RedirectToAction("Question", "Test", new { id = 1, TestId = TestId});

        }

        public IActionResult Resume(int TestId)
        {

            return RedirectToAction("Question", "Test", new { id = 1, TestId = TestId });

        }

        public IActionResult Videos()
        {
            var videoViewModel = new VideoViewModel();

            var attendantVideoList = _dbContext.tbl_AttendentTestVideos.Where(w => w.UserId == UserId).ToList();
            var videoList = _dbContext.tbl_Testvideos.ToList();
            VideoUtility _videoUtility = new VideoUtility(_appSettings, _dbContext);

            videoViewModel.Video1 = _videoUtility.GetVideo(attendantVideoList, 1, UserId);
            videoViewModel.Video2 = _videoUtility.GetVideo(attendantVideoList, 2, UserId);
            videoViewModel.Video3 = _videoUtility.GetVideo(attendantVideoList, 3, UserId);
            videoViewModel.Video4 = _videoUtility.GetVideo(attendantVideoList, 4, UserId);
            videoViewModel.Video5 = _videoUtility.GetVideo(attendantVideoList, 5, UserId);
            videoViewModel.Video6 = _videoUtility.GetVideo(attendantVideoList, 6, UserId);
            videoViewModel.Video7 = _videoUtility.GetVideo(attendantVideoList, 7, UserId);
            videoViewModel.Video8 = _videoUtility.GetVideo(attendantVideoList, 8, UserId);
            videoViewModel.Video9 = _videoUtility.GetVideo(attendantVideoList, 9, UserId);
            videoViewModel.Video10 = _videoUtility.GetVideo(attendantVideoList, 10, UserId);
            videoViewModel.Video11 = _videoUtility.GetVideo(attendantVideoList, 11, UserId);
            videoViewModel.Video12 = _videoUtility.GetVideo(attendantVideoList, 12, UserId);
            videoViewModel.Video13 = _videoUtility.GetVideo(attendantVideoList, 13, UserId);
            videoViewModel.Video14 = _videoUtility.GetVideo(attendantVideoList, 14, UserId);
            videoViewModel.Video15 = _videoUtility.GetVideo(attendantVideoList, 15, UserId);
            videoViewModel.Video16 = _videoUtility.GetVideo(attendantVideoList, 16, UserId);

            videoViewModel.Video1Duration = _videoUtility.GetVideoDuration(videoList, 1);
            videoViewModel.Video2Duration = _videoUtility.GetVideoDuration(videoList, 2);
            videoViewModel.Video3Duration = _videoUtility.GetVideoDuration(videoList, 3);
            videoViewModel.Video4Duration = _videoUtility.GetVideoDuration(videoList, 4);
            videoViewModel.Video5Duration = _videoUtility.GetVideoDuration(videoList, 5);
            videoViewModel.Video6Duration = _videoUtility.GetVideoDuration(videoList, 6);
            videoViewModel.Video7Duration = _videoUtility.GetVideoDuration(videoList, 7);
            videoViewModel.Video8Duration = _videoUtility.GetVideoDuration(videoList, 8);
            videoViewModel.Video9Duration = _videoUtility.GetVideoDuration(videoList, 9);
            videoViewModel.Video10Duration = _videoUtility.GetVideoDuration(videoList, 10);
            videoViewModel.Video11Duration = _videoUtility.GetVideoDuration(videoList, 11);
            videoViewModel.Video12Duration = _videoUtility.GetVideoDuration(videoList, 12);
            videoViewModel.Video13Duration = _videoUtility.GetVideoDuration(videoList, 13);
            videoViewModel.Video14Duration = _videoUtility.GetVideoDuration(videoList, 14);
            videoViewModel.Video15Duration = _videoUtility.GetVideoDuration(videoList, 15);
            videoViewModel.Video16Duration = _videoUtility.GetVideoDuration(videoList, 16);




            CareGiverVideoViewModel careGiverVideoViewModel = new CareGiverVideoViewModel();
            careGiverVideoViewModel.Videos = videoViewModel;
            var documents = new List<DocumentViewModel>();
            documents = (from document in _dbContext.tbl_Documents
                         where document.SeasonId == 1
                         select new DocumentViewModel
                         {
                             DocumentId = document.DocumentId,
                             Name = document.Name,
                             IsActive = document.IsActive,
                             CreatedDate = document.CreatedDate,
                             Description = document.Description

                         }).ToList();
            careGiverVideoViewModel.Documents = documents;
            return View(careGiverVideoViewModel);

        }

        public IActionResult Video()
        {
            return View();
        }


        [HttpPost]
        public JsonResult GetScore(int id)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var checkUserTest = _dbContext.tbl_UserTests.Where(w => w.UserId == UserId && w.TestId == id && w.IsReset == false).FirstOrDefault(); response.Status = "1";
                var questionsCount = _dbContext.tbl_Questions.Where(w => w.IsActive == true && w.TestId == id).Count();
                response.Status = "1";
                response.Message = "Your score is " + checkUserTest.QuestionsRight + " out of " + questionsCount;
            }
            catch (Exception ex)
            {
            }

            return Json(response);
        }

        [HttpPost]
        public JsonResult updateSession()
        {
            ResponseModel response = new ResponseModel();
            try
            {
                HttpContext.Session.SetString("ShowTest", "True");
                response.Message = "";
            }
            catch (Exception ex)
            {
            }

            return Json(response);
        }

        public IActionResult List()
        {
            var tests = new List<TestViewModel>();
            try
            {

                tests = (from test in _dbContext.tbl_Tests
                         join season in _dbContext.tbl_Seasons
                         on test.SeasonId equals season.Id
                         select new TestViewModel
                         {
                             Name = test.Name,
                             SeasonName = season.Name,
                             IsActive = test.IsActive
                         }).ToList();

            }
            catch (Exception ex)
            {

            }
            return View(tests);
        }

        [HttpPost]
        public ActionResult Add(TestViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    var checkTest = _dbContext.tbl_Tests.Where(w => w.Name == model.Name && w.SeasonId == model.SeasonId).FirstOrDefault();
                    if (checkTest == null)
                    {
                        Test test = new Test()
                        {
                            Name = model.Name,
                            SeasonId = model.SeasonId,
                            IsActive = true,
                            CreatedBy = UserId,
                            CreatedDate = DateTime.Now
                        };

                        _dbContext.tbl_Tests.Add(test);
                        _dbContext.SaveChanges();
                        ViewBag.SuccessMessage = "Test added successfully";
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Test name is already exists";

                    }
                }
            }
            catch (Exception ex)
            {
            }

            return View(model);
        }

        public ActionResult Add()
        {
            TestViewModel test = new TestViewModel();
            test.SeasonList = _dbContext.tbl_Seasons.Where(w => w.IsActive == true)
                            .Select(s => new SeasonViewModel
                            {
                                Name = s.Name,
                                Id = s.Id
                            }).ToList();
            return View(test);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

    }
}