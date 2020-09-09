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
    public class AttendantController : Controller
    {
        private readonly IOptions<Appsettings> _appSettings;
        private readonly DBContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession _session => _httpContextAccessor.HttpContext.Session;
        private int UserId = 0;
        public AttendantController(IOptions<Appsettings> appSettings, DBContext dbContext, IHttpContextAccessor HttpContextAccessor)
        {
            _appSettings = appSettings;
            _dbContext = dbContext;
            _httpContextAccessor = HttpContextAccessor;
            int.TryParse(_session.GetString("UserId"), out UserId);
        }

        public IActionResult Exam()
        {
            DbfunctionUtility dbfunction = new DbfunctionUtility(_appSettings);
            List<TestViewModel> TestList = _dbContext.tbl_Tests.Where(w=>w.IsActive??true).Where(w => w.SeasonId == 1)
                                        .Select(s => new TestViewModel
                                        {
                                            Id = s.Id,
                                            Name = s.Name,
                                        }).ToList();

            var checkTestCount = 0;
            foreach (var test in TestList)
            {

                var checkUserTest = _dbContext.tbl_UserTests.Where(w => w.UserId == UserId && w.TestId == test.Id && w.IsReset == false).FirstOrDefault();
                if (checkUserTest != null)
                {
                    checkTestCount++;
                    if (checkUserTest.IsLocked ?? false)
                    {
                        test.Status = "Completed";

                    }
                    else
                    {
                        test.Status = "Resume";
                    }

                }
                else
                {
                    var checkTotalDuration = _dbContext.tbl_Testvideos.Where(w=> w.TestId == test.Id).Sum(s => s.Duration) * 60;
                    var watchedTotalDuration = (from userVideo in _dbContext.tbl_AttendentTestVideos
                                                join testVideo in _dbContext.tbl_Testvideos
                                                on userVideo.VideoId equals testVideo.Id
                                                where userVideo.UserId ==  UserId
                                                && testVideo.TestId == test.Id
                                                select userVideo.Duration).Sum();

                    if (Math.Round(checkTotalDuration) <= Math.Round(watchedTotalDuration))
                    {
                        test.Status = "Start";
                        checkTestCount++;
                    }
                     

                }

            }

            if (checkTestCount==0)
            {
                return RedirectToAction("Videos", "Test");
            }

            return View(TestList);
        }

        [HttpPost]
        public JsonResult UpdateVideos(string vidName, string vid, bool isCompleted, int userId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                QuestionViewModel model = new QuestionViewModel();
                DbfunctionUtility dbfunction = new DbfunctionUtility(_appSettings);
                VideoUtility _videoUtility = new VideoUtility(_appSettings, _dbContext);

                if (vidName == "1")
                {
                    if (Convert.ToString(vid) != "0")
                    {
                        _videoUtility.UpdateVideo(vid, 1, userId, isCompleted);
                    }
                }

                if (vidName == "2")
                {
                    if (Convert.ToString(vid) != "0")
                    {
                        _videoUtility.UpdateVideo(vid, 2, userId, isCompleted);
                    }
                }

                if (vidName == "3")
                {
                    if (Convert.ToString(vid) != "0")
                    {
                        _videoUtility.UpdateVideo(vid, 3, userId, isCompleted);
                    }
                }

                if (vidName == "4")
                {
                    if (Convert.ToString(vid) != "0")
                    {
                        _videoUtility.UpdateVideo(vid, 4, userId, isCompleted);
                    }
                }

                if (vidName == "5")
                {
                    if (Convert.ToString(vid) != "0")
                    {
                        _videoUtility.UpdateVideo(vid, 5, userId, isCompleted);
                    }
                }

                if (vidName == "6")
                {
                    if (Convert.ToString(vid) != "0")
                    {
                        _videoUtility.UpdateVideo(vid, 6, userId, isCompleted);
                    }
                }

                if (vidName == "7")
                {
                    if (Convert.ToString(vid) != "0")
                    {
                        _videoUtility.UpdateVideo(vid, 7, userId, isCompleted);
                    }
                }

                if (vidName == "8")
                {
                    if (Convert.ToString(vid) != "0")
                    {
                        _videoUtility.UpdateVideo(vid, 8, userId, isCompleted);
                    }
                }

                if (vidName == "9")
                {
                    if (Convert.ToString(vid) != "0")
                    {
                        _videoUtility.UpdateVideo(vid, 9, userId, isCompleted);
                    }
                }

                if (vidName == "10")
                {
                    if (Convert.ToString(vid) != "0")
                    {
                        _videoUtility.UpdateVideo(vid, 10, userId, isCompleted);
                    }
                }

                if (vidName == "11")
                {
                    if (Convert.ToString(vid) != "0")
                    {
                        _videoUtility.UpdateVideo(vid, 11, userId, isCompleted);
                    }
                }

                if (vidName == "12")
                {
                    if (Convert.ToString(vid) != "0")
                    {
                        _videoUtility.UpdateVideo(vid, 12, userId, isCompleted);
                    }
                }

                if (vidName == "13")
                {
                    if (Convert.ToString(vid) != "0")
                    {
                        _videoUtility.UpdateVideo(vid, 13, userId, isCompleted);
                    }
                }

                if (vidName == "14")
                {
                    if (Convert.ToString(vid) != "0")
                    {
                        _videoUtility.UpdateVideo(vid, 14, userId, isCompleted);
                    }
                }

                if (vidName == "15")
                {
                    if (Convert.ToString(vid) != "0")
                    {
                        _videoUtility.UpdateVideo(vid, 15, userId, isCompleted);
                    }
                }

                if (vidName == "16")
                {
                    if (Convert.ToString(vid) != "0")
                    {
                        _videoUtility.UpdateVideo(vid, 16, userId, isCompleted);
                    }
                }

                if (isCompleted)
                {
                    var videoId = Convert.ToInt32(vidName);
                    var testId = _dbContext.tbl_Testvideos.Where(w => w.Id == videoId).Select(s => s.TestId).FirstOrDefault();
                    var testVideoList = _dbContext.tbl_Testvideos.Where(w => w.TestId == testId).Select(s => s.Id).ToList();
                    var checkUserTestVideos = _dbContext.tbl_AttendentTestVideos.Where(w => w.UserId == userId && testVideoList.Contains(w.VideoId)).Select(s => s.IsCompleted ?? false).ToList();
                    if (checkUserTestVideos.Where(w => w == false).Count() == 0 && checkUserTestVideos.Count()==testVideoList.Count())
                    {
                        response.Message = "C";
                        HttpContext.Session.SetString("ShowTest", "True");

                    }
                    else
                    {
                        response.Message = "I";
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return Json(response);
        }

        

        public async Task<IActionResult> Download(string id)
        {

            if (id == null)
                return Content("filename not present");

            int idInt = 0;
            int.TryParse(id, out idInt);

            var document = _dbContext.tbl_Documents.Where(w => w.DocumentId == idInt).FirstOrDefault();

            if (document == null)
                return Content("filename not present");

            DocumentUtility _documentUtility = new DocumentUtility();

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Document", document.FileName);
            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open)) { stream.CopyTo(memory); }
            memory.Position = 0;
            return File(memory, _documentUtility.GetContentType(path), Path.GetFileName(path));
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}