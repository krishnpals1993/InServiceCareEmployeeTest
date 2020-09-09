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

        public IActionResult Question(int id, int TestId)
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
                var questionList = _testUtility.GetUserQuestions(UserId, TestId);
                model = questionList.Where(w => w.SeqNo == id).FirstOrDefault();
                model.TestId = TestId;
                model.MaxSequenceId = questionList.LastOrDefault().SeqNo;
                model.MinSequenceId = questionList.FirstOrDefault().SeqNo;

            }

            model.TestName = _dbContext.tbl_Tests.Where(w => w.Id == TestId).Select(s => s.Name).FirstOrDefault();

            return View(model);
        }

        [HttpPost]
        public IActionResult Question(TestQuestionViewModel model, string submit)
        {
            int TestId = model.TestId;
            DbfunctionUtility dbfunction = new DbfunctionUtility(_appSettings);
            TestUtility _testUtility = new TestUtility(_appSettings, _dbContext);
            var checkUserTest = _dbContext.tbl_UserTests.Where(w => w.UserId == UserId && w.TestId == TestId && w.IsReset == false).FirstOrDefault();
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

                var questionList = _testUtility.GetUserQuestions(UserId, TestId);

                if (model.SeqNo != model.MaxSequenceId)
                {
                    model = questionList.Where(w => w.SeqNo == model.SeqNo + 1).FirstOrDefault();
                    model.MaxSequenceId = questionList.LastOrDefault().SeqNo;
                    model.MinSequenceId = questionList.FirstOrDefault().SeqNo;


                }
            }
            if (submit == "Previous")
            {

                var questionList = _testUtility.GetUserQuestions(UserId, TestId);
                model = questionList.Where(w => w.SeqNo == model.SeqNo - 1).FirstOrDefault();
                model.MaxSequenceId = questionList.LastOrDefault().SeqNo;
                model.MinSequenceId = questionList.FirstOrDefault().SeqNo;


            }
            if (submit == "Next")
            {

                var questionList = _testUtility.GetUserQuestions(UserId, TestId);
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

            return RedirectToAction("Question", "Test", new { id = 1, TestId = TestId });

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

            VideoTimeDuration VideoDetail1 = _videoUtility.GetVideo(attendantVideoList, 1, UserId);
            VideoTimeDuration VideoDetail2 = _videoUtility.GetVideo(attendantVideoList, 2, UserId);
            VideoTimeDuration VideoDetail3 = _videoUtility.GetVideo(attendantVideoList, 3, UserId);
            VideoTimeDuration VideoDetail4 = _videoUtility.GetVideo(attendantVideoList, 4, UserId);
            VideoTimeDuration VideoDetail5 = _videoUtility.GetVideo(attendantVideoList, 5, UserId);
            VideoTimeDuration VideoDetail6 = _videoUtility.GetVideo(attendantVideoList, 6, UserId);
            VideoTimeDuration VideoDetail7 = _videoUtility.GetVideo(attendantVideoList, 7, UserId);
            VideoTimeDuration VideoDetail8 = _videoUtility.GetVideo(attendantVideoList, 8, UserId);
            VideoTimeDuration VideoDetail9 = _videoUtility.GetVideo(attendantVideoList, 9, UserId);
            VideoTimeDuration VideoDetail10 = _videoUtility.GetVideo(attendantVideoList, 10, UserId);
            VideoTimeDuration VideoDetail11 = _videoUtility.GetVideo(attendantVideoList, 11, UserId);
            VideoTimeDuration VideoDetail12 = _videoUtility.GetVideo(attendantVideoList, 12, UserId);
            VideoTimeDuration VideoDetail13 = _videoUtility.GetVideo(attendantVideoList, 13, UserId);
            VideoTimeDuration VideoDetail14 = _videoUtility.GetVideo(attendantVideoList, 14, UserId);
            VideoTimeDuration VideoDetail15 = _videoUtility.GetVideo(attendantVideoList, 15, UserId);
            VideoTimeDuration VideoDetail16 = _videoUtility.GetVideo(attendantVideoList, 16, UserId);

            videoViewModel.Video1 = VideoDetail1.Duration;
            videoViewModel.Video2 = VideoDetail2.Duration;
            videoViewModel.Video3 = VideoDetail3.Duration;
            videoViewModel.Video4 = VideoDetail4.Duration;
            videoViewModel.Video5 = VideoDetail5.Duration;
            videoViewModel.Video6 = VideoDetail6.Duration;
            videoViewModel.Video7 = VideoDetail7.Duration;
            videoViewModel.Video8 = VideoDetail8.Duration;
            videoViewModel.Video9 = VideoDetail9.Duration;
            videoViewModel.Video10 = VideoDetail10.Duration;
            videoViewModel.Video11 = VideoDetail11.Duration;
            videoViewModel.Video12 = VideoDetail12.Duration;
            videoViewModel.Video13 = VideoDetail13.Duration;
            videoViewModel.Video14 = VideoDetail14.Duration;
            videoViewModel.Video15 = VideoDetail15.Duration;
            videoViewModel.Video16 = VideoDetail16.Duration;


            videoViewModel.Video1Completed = VideoDetail1.Completed;
            videoViewModel.Video2Completed = VideoDetail2.Completed;
            videoViewModel.Video3Completed = VideoDetail3.Completed;
            videoViewModel.Video4Completed = VideoDetail4.Completed;
            videoViewModel.Video5Completed = VideoDetail5.Completed;
            videoViewModel.Video6Completed = VideoDetail6.Completed;
            videoViewModel.Video7Completed = VideoDetail7.Completed;
            videoViewModel.Video8Completed = VideoDetail8.Completed;
            videoViewModel.Video9Completed = VideoDetail9.Completed;
            videoViewModel.Video10Completed = VideoDetail10.Completed;
            videoViewModel.Video11Completed = VideoDetail11.Completed;
            videoViewModel.Video12Completed = VideoDetail12.Completed;
            videoViewModel.Video13Completed = VideoDetail13.Completed;
            videoViewModel.Video14Completed = VideoDetail14.Completed;
            videoViewModel.Video15Completed = VideoDetail15.Completed;
            videoViewModel.Video16Completed = VideoDetail16.Completed;


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
            AdminVideoViewModel model = new AdminVideoViewModel();
            model.TestList = (from test in _dbContext.tbl_Tests
                              join season in _dbContext.tbl_Seasons
                              on test.SeasonId equals season.Id
                              where test.IsActive ?? true
                              select new TestViewModel
                              {
                                  Name = test.Name + " (" + season.Name + ")",
                                  Id = test.Id,
                                  IsActive = test.IsActive,
                                

                              }).ToList();

            var checkVideos = (from test in _dbContext.tbl_Tests
                              join video in _dbContext.tbl_Testvideos
                              on test.Id equals video.TestId
                              into video
                              from video1 in video.DefaultIfEmpty()
                              where test.IsActive ?? true
                              select new TestViewModel
                              {
                                
                                  Id = test.Id,
                                  VideoId = video1.Id

                              }).ToList();

            model.Video1 = checkVideos.Where(w => w.VideoId == 1).Select(s => s.Id).FirstOrDefault();
            model.Video2 = checkVideos.Where(w => w.VideoId == 2).Select(s => s.Id).FirstOrDefault();
            model.Video3 = checkVideos.Where(w => w.VideoId == 3).Select(s => s.Id).FirstOrDefault();
            model.Video4 = checkVideos.Where(w => w.VideoId == 4).Select(s => s.Id).FirstOrDefault();
            model.Video5 = checkVideos.Where(w => w.VideoId == 5).Select(s => s.Id).FirstOrDefault();
            model.Video6 = checkVideos.Where(w => w.VideoId == 6).Select(s => s.Id).FirstOrDefault();
            model.Video7 = checkVideos.Where(w => w.VideoId == 7).Select(s => s.Id).FirstOrDefault();
            model.Video8 = checkVideos.Where(w => w.VideoId == 8).Select(s => s.Id).FirstOrDefault();
            model.Video9 = checkVideos.Where(w => w.VideoId == 9).Select(s => s.Id).FirstOrDefault();
            model.Video10 =checkVideos.Where(w => w.VideoId == 10).Select(s => s.Id).FirstOrDefault();
            model.Video11 =checkVideos.Where(w => w.VideoId == 11).Select(s => s.Id).FirstOrDefault();
            model.Video12 =checkVideos.Where(w => w.VideoId == 12).Select(s => s.Id).FirstOrDefault();
            model.Video13 =checkVideos.Where(w => w.VideoId == 13).Select(s => s.Id).FirstOrDefault();
            model.Video14 =checkVideos.Where(w => w.VideoId == 14).Select(s => s.Id).FirstOrDefault();
            model.Video15 =checkVideos.Where(w => w.VideoId == 15).Select(s => s.Id).FirstOrDefault();
            model.Video16 =checkVideos.Where(w => w.VideoId == 16).Select(s => s.Id).FirstOrDefault();

            return View(model);

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
        public JsonResult updateSession(int vid, int userId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var testId = _dbContext.tbl_Testvideos.Where(w => w.Id == vid).Select(s => s.TestId).FirstOrDefault();
                var testVideoList = _dbContext.tbl_Testvideos.Where(w => w.TestId == testId).Select(s => s.Id).ToList();
                var checkUserTestVideos = _dbContext.tbl_AttendentTestVideos.Where(w => w.UserId == userId && testVideoList.Contains(w.Id)).Select(s => s.IsCompleted??false).ToList();
                if (checkUserTestVideos.Where(w=> w==false).Count()>0)
                {
                    response.Message = "I";
                }
                else
                {
                    response.Message = "C";
                    HttpContext.Session.SetString("ShowTest", "True");
                }
                
              
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
                             Id = test.Id,
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
                        model.SeasonList = _dbContext.tbl_Seasons.Where(w => w.IsActive == true)
                            .Select(s => new SeasonViewModel
                            {
                                Name = s.Name,
                                Id = s.Id
                            }).ToList();
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

        public ActionResult Edit(string id)
        {
            TestViewModel model = new TestViewModel();
            try
            {
                if (ModelState.IsValid)
                {
                    var testId = Convert.ToInt32(id);

                    var checkTest = _dbContext.tbl_Tests.Where(w => w.Id == testId).FirstOrDefault();
                    if (checkTest != null)
                    {
                        model.Id = checkTest.Id;
                        model.Name = checkTest.Name;
                        model.SeasonId = checkTest.SeasonId;
                        model.SeasonList = _dbContext.tbl_Seasons.Where(w => w.IsActive == true)
                          .Select(s => new SeasonViewModel
                          {
                              Name = s.Name,
                              Id = s.Id
                          }).ToList();

                    }
                    else
                    {
                        return RedirectToAction("List", "Test");
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return View(model);

        }

        [HttpPost]
        public ActionResult Edit(TestViewModel model)
        {
            try
            {

                if (ModelState.IsValid)
                {

                    var checkTest = _dbContext.tbl_Tests.Where(w => w.Name == model.Name && w.Id != model.Id).FirstOrDefault();
                    if (checkTest == null)
                    {
                        var testDetail = _dbContext.tbl_Tests.Where(w => w.Id == model.Id).FirstOrDefault();

                        testDetail.Name = model.Name;
                        testDetail.SeasonId = model.SeasonId;
                        testDetail.ModifiedBy = UserId;
                        testDetail.ModifiedDate = DateTime.Now;
                        _dbContext.SaveChanges();
                        ViewBag.SuccessMessage = "Test updated successfully";


                    }
                    else
                    {
                        model.SeasonList = _dbContext.tbl_Seasons.Where(w => w.IsActive == true)
                            .Select(s => new SeasonViewModel
                            {
                                Name = s.Name,
                                Id = s.Id
                            }).ToList();
                        ViewBag.ErrorMessage = "Test name is already exists";

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
                int iId = Convert.ToInt32(id);
                QuestionViewModel model = new QuestionViewModel();
                DbfunctionUtility dbfunction = new DbfunctionUtility(_appSettings);
                var checkTest = _dbContext.tbl_UserTests.Where(w => w.TestId == iId).Count();
                if (checkTest == 0)
                {
                    var query = " update tests set IsActive = 0   where Id ='" + id + "' ;";
                    DataSet ds = dbfunction.GetDataset(query);
                    response.Status = "1";
                    response.Message = "Test deleted successfully";
                }
                else
                {
                    response.Status = "0";
                    response.Message = "Test has been given by CareGiver for this test";

                }

            }
            catch (Exception ex)
            {
            }

            return Json(response);
        }

        [HttpPost]
        public JsonResult UpdateVideoTest(int vid, int testId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var checkVideo = _dbContext.tbl_Testvideos.Where(w => w.Id == vid).FirstOrDefault();
                if (checkVideo != null)
                {
                    checkVideo.TestId = testId;
                    checkVideo.ModifiedBy = UserId;
                    checkVideo.ModifiedDate = DateTime.Now;
                    _dbContext.SaveChanges();
                    response.Status = "1";
                    response.Message = "Video mapped successfully";
                }
                else
                {
                    response.Status = "0";
                    response.Message = "Error occurred";

                }

            }
            catch (Exception ex)
            {
            }

            return Json(response);
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

    }
}