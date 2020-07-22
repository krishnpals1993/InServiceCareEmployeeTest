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
    public class UserController : Controller
    {
        private readonly IOptions<Appsettings> _appSettings;
        private readonly DBContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession _session => _httpContextAccessor.HttpContext.Session;
        private int UserId = 0;
        public UserController(IOptions<Appsettings> appSettings, DBContext dbContext, IHttpContextAccessor HttpContextAccessor)
        {
            _appSettings = appSettings;
            _dbContext = dbContext;
            _httpContextAccessor = HttpContextAccessor;
            int.TryParse(_session.GetString("UserId"), out UserId);
        }


        public IActionResult List()
        {
            var users = new List<UserViewModel>();
            try
            {

                users = (from user in _dbContext.tbl_Users
                         join role in _dbContext.tbl_Roles
                         on user.RoleId equals role.RoleId
                         select new UserViewModel
                         {
                            // RoleId = role.RoleId,
                             RoleName = role.Rolename,
                             UserId = user.UserId,
                             Username = user.Username,
                             Email = user.Email,
                             IsActive = user.IsActive
                         }).ToList();

            }
            catch (Exception ex)
            {

            }
            return View(users);
        }

        public ActionResult Add()
        {
            UserViewModel user = new UserViewModel();
            user.RoleList = _dbContext.tbl_Roles.Where(w => w.IsActive == true && w.Rolename != "CareGiver")
                            .Select(s => new RoleViewModel
                            {
                                Rolename = s.Rolename,
                                RoleId = s.RoleId
                            }).ToList();
            return View(user);
        }

        public ActionResult Edit(string id)
        {
            UserViewModel model = new UserViewModel();
            try
            {
                if (ModelState.IsValid)
                {
                    var userId = Convert.ToInt32(id);
                    model.RoleList = _dbContext.tbl_Roles.Where(w => w.IsActive == true && w.Rolename != "CareGiver")
                          .Select(s => new RoleViewModel
                          {
                              Rolename = s.Rolename,
                              RoleId = s.RoleId
                          }).ToList();
                    var checkUser = _dbContext.tbl_Users.Where(w => w.UserId == userId ).FirstOrDefault();
                    if (checkUser != null)
                    {
                        model.RoleId = checkUser.RoleId.ToString();
                        model.Username = checkUser.Username;
                        model.Email = checkUser.Email;
                        model.UserId = checkUser.UserId;
                    }
                    else
                    {
                        return RedirectToAction("UserList", "UserSetting");
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return View(model);

        }

        [HttpPost]
        public ActionResult Add(UserViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.Email = model.Email.Trim();
                    var checkRole = _dbContext.tbl_Users.Where(w => w.Username == model.Email).FirstOrDefault();
                    if (checkRole == null)
                    {
                        model.iRoleId = Convert.ToInt32(model.RoleId);
                        User user = new User()
                        {
                            Username = model.Email,
                            Password = model.Password,
                            Email = model.Email,
                            RoleId = model.iRoleId,
                            IsActive = true,
                            CreatedBy = UserId,
                            CreatedDate = DateTime.Now
                        };

                        _dbContext.tbl_Users.Add(user);
                        _dbContext.SaveChanges();
                        ViewBag.SuccessMessage = "User added successfully";
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "User name is already exists";

                    }
                }
            }
            catch (Exception ex)
            {
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(UserViewModel model)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    model.Email = model.Email.Trim();
                    var checkUser = _dbContext.tbl_Users.Where(w => w.Username == model.Email && w.UserId != model.UserId).FirstOrDefault();
                    if (checkUser == null)
                    {
                        var userDetail = _dbContext.tbl_Users.Where(w => w.UserId == model.UserId).FirstOrDefault();

                        userDetail.Username = model.Email;
                        userDetail.Email = model.Email;
                        userDetail.Password = model.Password;
                        userDetail.RoleId =  Convert.ToInt32(model.RoleId); 
                        userDetail.CreatedBy = UserId;
                        userDetail.CreatedDate = DateTime.Now;
                        _dbContext.SaveChanges();
                        ViewBag.SuccessMessage = "User updated successfully";
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "User name is already exists";

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
                var query = " update users set IsActive = 0   where UserId ='" + id + "' ;";
                DataSet ds = dbfunction.GetDataset(query);
                response.Status = "1";
                response.Message = "User deleted successfully";
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