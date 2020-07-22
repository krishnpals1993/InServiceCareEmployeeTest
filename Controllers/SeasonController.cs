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
                            Name = season.Name,
                            StartDay= season.StartDay,
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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

    }
}