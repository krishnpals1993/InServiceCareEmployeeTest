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
    public class HrGroupUserMappingController : Controller
    {
        private readonly IOptions<Appsettings> _appSettings;
        private readonly DBContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession _session => _httpContextAccessor.HttpContext.Session;
        private int UserId = 0;
        public HrGroupUserMappingController(IOptions<Appsettings> appSettings, DBContext dbContext, IHttpContextAccessor HttpContextAccessor)
        {
            _appSettings = appSettings;
            _dbContext = dbContext;
            _httpContextAccessor = HttpContextAccessor;
            int.TryParse(_session.GetString("UserId"), out UserId);
        }

        public IActionResult List()
        {
            var users = new List<UserViewModel>();
            var hrRole = _dbContext.tbl_Roles.Where(w => w.Rolename.ToLower() == "hr").Select(s => s.RoleId).FirstOrDefault();
            try
            {

                users = (from user in _dbContext.tbl_Users
                         join hrGroup in _dbContext.tbl_HrGroups
                         on user.HrGroupId equals hrGroup.Id
                         into hrGroup
                         from hrGroup1 in hrGroup.DefaultIfEmpty()
                         where user.RoleId == hrRole
                         select new UserViewModel
                         {
                             UserId = user.UserId,
                             Username = user.Username,
                             HrGroupName = hrGroup1.Name
                             
                         }).ToList();

            }
            catch (Exception ex)
            {

            }
            return View(users);
        }

       

        public ActionResult Edit(string id)
        {
            HrGroupUserMappingViewModel model = new HrGroupUserMappingViewModel();
            try
            {
                if (ModelState.IsValid)
                {
                    var userId = Convert.ToInt32(id);

                    var checkUser = _dbContext.tbl_Users.Where(w => w.UserId == userId).FirstOrDefault();
                    if (checkUser != null)
                    {
                        model.UserId = checkUser.UserId;
                        model.Username = checkUser.Username;
                        model.HrGroupId = checkUser.HrGroupId ?? 0;
                        model.HrGroupList = _dbContext.tbl_HrGroups.Where(w => w.IsActive == true)
                        .Select(s => new HrGroupViewModel
                        {
                            Id = s.Id,
                            Name = s.Name

                        }).ToList();
                    }
                    else
                    {
                        return RedirectToAction("List", "HrGroupUserMapping");
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return View(model);

        }

        [HttpPost]
        public ActionResult Edit(HrGroupUserMappingViewModel model)
        {
            try
            {

                if (ModelState.IsValid)
                {

                    var checkHrGroup = _dbContext.tbl_Users.Where(w => w.UserId == model.UserId).FirstOrDefault();
                    if (checkHrGroup == null)
                    {

                        ViewBag.ErrorMessage = "User not exists";

                    }
                    else
                    {
                        checkHrGroup.HrGroupId = model.HrGroupId;
                        _dbContext.SaveChanges();
                        ViewBag.SuccessMessage = "Hr group updated successfully";
                    }
                }

                model.HrGroupList = _dbContext.tbl_HrGroups.Where(w => w.IsActive == true)
               .Select(s => new HrGroupViewModel
               {
                   Id = s.Id,
                   Name = s.Name

               }).ToList();

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