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
    public class HrGroupController : Controller
    {
        private readonly IOptions<Appsettings> _appSettings;
        private readonly DBContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession _session => _httpContextAccessor.HttpContext.Session;
        private int UserId = 0;
        public HrGroupController(IOptions<Appsettings> appSettings, DBContext dbContext, IHttpContextAccessor HttpContextAccessor)
        {
            _appSettings = appSettings;
            _dbContext = dbContext;
            _httpContextAccessor = HttpContextAccessor;
            int.TryParse(_session.GetString("UserId"), out UserId);
        }

        public IActionResult List()
        {
            var hrGroups = new List<HrGroupViewModel>();
            try
            {

                hrGroups = (from hrGroup in _dbContext.tbl_HrGroups
                           select new HrGroupViewModel
                           {
                               Id = hrGroup.Id,
                               Name = hrGroup.Name,
                               IsActive = hrGroup.IsActive

                           }).ToList();

            }
            catch (Exception ex)
            {

            }
            return View(hrGroups);
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Add(HrGroupViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    var checkGroup = _dbContext.tbl_HrGroups.Where(w => w.Name == model.Name).FirstOrDefault();
                    if (checkGroup == null)
                    {

                        HrGroup hrGroup = new HrGroup()
                        {
                            Name = model.Name,
                            IsActive = true,
                            CreatedBy = UserId,
                            CreatedDate = DateTime.Now
                        };

                        _dbContext.tbl_HrGroups.Add(hrGroup);
                        _dbContext.SaveChanges();
                        ViewBag.SuccessMessage = "Hr group added successfully";
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Hr group name is already exists";

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
            HrGroupViewModel model = new HrGroupViewModel();
            try
            {
                if (ModelState.IsValid)
                {
                    var seasonId = Convert.ToInt32(id);

                    var checkSeason = _dbContext.tbl_HrGroups.Where(w => w.Id == seasonId).FirstOrDefault();
                    if (checkSeason != null)
                    {
                        model.Id = checkSeason.Id;
                        model.Name = checkSeason.Name;
                    }
                    else
                    {
                        return RedirectToAction("List", "HrGroup");
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return View(model);

        }

        [HttpPost]
        public ActionResult Edit(HrGroupViewModel model)
        {
            try
            {

                if (ModelState.IsValid)
                {

                    var checkHrGroup = _dbContext.tbl_HrGroups.Where(w => w.Name == model.Name && w.Id != model.Id).FirstOrDefault();
                    if (checkHrGroup == null)
                    {
                        var userDetail = _dbContext.tbl_HrGroups.Where(w => w.Id == model.Id).FirstOrDefault();

                        userDetail.Name = model.Name;
                        userDetail.ModifiedBy = UserId;
                        userDetail.ModifiedDate = DateTime.Now;
                        _dbContext.SaveChanges();
                        ViewBag.SuccessMessage = "Hr group updated successfully";


                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Hr group name is already exists";

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
                DbfunctionUtility dbfunction = new DbfunctionUtility(_appSettings);
                var query = " update hr_groups set IsActive = 0   where Id ='" + id + "' ;";
                DataSet ds = dbfunction.GetDataset(query);
                response.Status = "1";
                response.Message = "Hr group deleted successfully";


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