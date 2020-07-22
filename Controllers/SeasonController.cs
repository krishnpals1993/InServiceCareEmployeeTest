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
    public class SeasonController : Controller
    {
        private readonly IOptions<Appsettings> _appSettings;
        private readonly DBContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession _session => _httpContextAccessor.HttpContext.Session;
        private int UserId = 0;
        public SeasonController(IOptions<Appsettings> appSettings, DBContext dbContext, IHttpContextAccessor HttpContextAccessor)
        {
            _appSettings = appSettings;
            _dbContext = dbContext;
            _httpContextAccessor = HttpContextAccessor;
            int.TryParse(_session.GetString("UserId"), out UserId);
        }

        public IActionResult List()
        {
            var seasons = new List<SeasonViewModel>();
            try
            {

                seasons = (from season in _dbContext.tbl_Seasons
                           select new SeasonViewModel
                           {
                               Id = season.Id,
                               Name = season.Name,
                               StartDay = season.StartDay,
                               EndDay = season.EndDay,
                               IsActive = season.IsActive

                           }).ToList();

            }
            catch (Exception ex)
            {

            }
            return View(seasons);
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Add(SeasonViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    var checkRole = _dbContext.tbl_Seasons.Where(w => w.Name == model.Name).FirstOrDefault();
                    if (checkRole == null)
                    {

                        Season season = new Season()
                        {
                            Name = model.Name,
                            StartDay = model.StartDay,
                            EndDay = model.EndDay,
                            IsActive = true,
                            CreatedBy = UserId,
                            CreatedDate = DateTime.Now
                        };

                        _dbContext.tbl_Seasons.Add(season);
                        _dbContext.SaveChanges();
                        ViewBag.SuccessMessage = "Season added successfully";
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Season name is already exists";

                    }
                }
            }
            catch (Exception ex)
            {
            }

            return View(model);
        }

        public ActionResult Edit(string id)
        {
            SeasonViewModel model = new SeasonViewModel();
            try
            {
                if (ModelState.IsValid)
                {
                    var seasonId = Convert.ToInt32(id);

                    var checkSeason = _dbContext.tbl_Seasons.Where(w => w.Id == seasonId).FirstOrDefault();
                    if (checkSeason != null)
                    {
                        model.Id = checkSeason.Id;
                        model.Name = checkSeason.Name;
                        model.StartDay = checkSeason.StartDay;
                        model.EndDay = checkSeason.EndDay;
                    }
                    else
                    {
                        return RedirectToAction("List", "Season");
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return View(model);

        }

        [HttpPost]
        public ActionResult Edit(SeasonViewModel model)
        {
            try
            {

                if (ModelState.IsValid)
                {

                    var checkSeason = _dbContext.tbl_Seasons.Where(w => w.Name == model.Name && w.Id != model.Id).FirstOrDefault();
                    if (checkSeason == null)
                    {
                        var userDetail = _dbContext.tbl_Seasons.Where(w => w.Id == model.Id).FirstOrDefault();

                        userDetail.Name = model.Name;
                        userDetail.StartDay = model.StartDay;
                        userDetail.EndDay = model.EndDay;
                        userDetail.ModifiedBy = UserId;
                        userDetail.ModifiedDate = DateTime.Now;
                        _dbContext.SaveChanges();
                        ViewBag.SuccessMessage = "Season updated successfully";


                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Season name is already exists";

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
                var checkTest = _dbContext.tbl_Tests.Where(w => w.SeasonId == iId).Count();
                if (checkTest == 0)
                {
                    var query = " update seasons set IsActive = 0   where Id ='" + id + "' ;";
                    DataSet ds = dbfunction.GetDataset(query);
                    response.Status = "1";
                    response.Message = "User deleted successfully";
                }
                else
                {
                    response.Status = "0";
                    response.Message = "Test has been created for this season, To delete it please delete test for this season";

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