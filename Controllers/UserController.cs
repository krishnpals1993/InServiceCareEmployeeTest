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
                         join hrGroup in _dbContext.tbl_HrGroups
                         on user.HrGroupId equals hrGroup.Id
                         into hrGroup
                         from hrGroup1 in hrGroup.DefaultIfEmpty()
                         join attedent in _dbContext.tbl_Attendants
                         on user.UserId equals attedent.UserId
                         into attedent
                         from attedent1 in attedent.DefaultIfEmpty()
                         join hrGroup2 in _dbContext.tbl_HrGroups
                         on attedent1.HrGroupId equals hrGroup2.Id
                         into hrGroup2
                         from hrGroup3 in hrGroup2.DefaultIfEmpty()

                         select new UserViewModel
                         {
                             // RoleId = role.RoleId,
                             RoleName = role.Rolename,
                             UserId = user.UserId,
                             Username = user.Username,
                             Email = user.Email,
                             IsActive = user.IsActive,
                             HrGroupName = hrGroup1.Name ?? hrGroup3.Name
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
            user.RoleList = _dbContext.tbl_Roles.Where(w => w.IsActive == true)
                            .Select(s => new RoleViewModel
                            {
                                Rolename = s.Rolename,
                                RoleId = s.RoleId
                            }).ToList();
            user.HrGroupList = _dbContext.tbl_HrGroups.Where(w => w.IsActive == true)
              .Select(s => new HrGroupViewModel
              {
                  Id = s.Id,
                  Name = s.Name

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
                    model.RoleList = _dbContext.tbl_Roles.Where(w => w.IsActive == true)
                          .Select(s => new RoleViewModel
                          {
                              Rolename = s.Rolename,
                              RoleId = s.RoleId
                          }).ToList();
                    model.HrGroupList = _dbContext.tbl_HrGroups.Where(w => w.IsActive == true)
                          .Select(s => new HrGroupViewModel
                          {
                              Id = s.Id,
                              Name = s.Name

                          }).ToList();
                    var checkUser = _dbContext.tbl_Users.Where(w => w.UserId == userId).FirstOrDefault();
                    if (checkUser != null)
                    {
                        model.RoleId = checkUser.RoleId.ToString();
                        model.Username = checkUser.Username;
                        model.Email = checkUser.Email;
                        model.UserId = checkUser.UserId;

                        if (model.RoleId == "1")
                        {
                            var checkCareGiver = _dbContext.tbl_Attendants.Where(w => w.UserId == model.UserId).FirstOrDefault();
                            if (checkCareGiver != null)
                            {
                                model.FirstName = checkCareGiver.FirstName;
                                model.MiddleName = checkCareGiver.MiddleName;
                                model.LastName = checkCareGiver.LastName;
                                model.EmployeeNo = checkCareGiver.EmployeeNo;
                                model.HrGroupId = checkCareGiver.HrGroupId ?? 0;

                            }
                        }

                    }
                    else
                    {
                        return RedirectToAction("List", "User");
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

                        if (model.iRoleId == 1)
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
                                UserId = user.UserId,
                                HrGroupId = user.HrGroupId

                            };

                            _dbContext.tbl_Attendants.Add(attendant);
                            _dbContext.SaveChanges();
                        }


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
            model.RoleList = _dbContext.tbl_Roles.Where(w => w.IsActive == true)
                          .Select(s => new RoleViewModel
                          {
                              Rolename = s.Rolename,
                              RoleId = s.RoleId
                          }).ToList();
            model.HrGroupList = _dbContext.tbl_HrGroups.Where(w => w.IsActive == true)
                  .Select(s => new HrGroupViewModel
                  {
                      Id = s.Id,
                      Name = s.Name

                  }).ToList();
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
                        userDetail.RoleId = Convert.ToInt32(model.RoleId);
                        userDetail.CreatedBy = UserId;
                        userDetail.CreatedDate = DateTime.Now;
                        _dbContext.SaveChanges();
                        ViewBag.SuccessMessage = "User updated successfully";

                        if (model.RoleId == "1")
                        {
                            var checkCareGiver = _dbContext.tbl_Attendants.Where(w => w.UserId == model.UserId).FirstOrDefault();
                            if (checkCareGiver != null)
                            {
                                checkCareGiver.FirstName = model.FirstName;
                                checkCareGiver.MiddleName = model.MiddleName;
                                checkCareGiver.LastName = model.LastName;
                                checkCareGiver.EmployeeNo = model.EmployeeNo;
                                checkCareGiver.Email = model.Email;
                                checkCareGiver.HrGroupId = model.HrGroupId;
                                _dbContext.SaveChanges();
                            }
                            else
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
                                    UserId = model.UserId

                                };

                                _dbContext.tbl_Attendants.Add(attendant);
                                _dbContext.SaveChanges();
                            }
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "User name is already exists";

                    }
                }
                model.RoleList = _dbContext.tbl_Roles.Where(w => w.IsActive == true)
                         .Select(s => new RoleViewModel
                         {
                             Rolename = s.Rolename,
                             RoleId = s.RoleId
                         }).ToList();
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

        public ActionResult PermissionList()
        {
            List<UserPermissionViewModel> UserPermissionList = new List<UserPermissionViewModel>();


            UserPermissionList = _dbContext.tbl_Roles.Where(w => w.IsActive == true)
                            .Select(s => new UserPermissionViewModel
                            {
                                Rolename = s.Rolename,
                                RoleId = s.RoleId
                            }).ToList();

            foreach (var item in UserPermissionList)
            {
                item.Menus = (from claim in _dbContext.tbl_Userclaim
                              join menu in _dbContext.tbl_Menus
                              on claim.MenuId equals menu.MenuId
                              where claim.RoleId == item.RoleId
                              select new MenuViewModel
                              {
                                  Name = menu.Name + " (" + menu.Parent + ")"
                              }).ToList();
            }

            return View(UserPermissionList);
        }

        public ActionResult AddPermission(int id)
        {
            UserPermissionViewModel model = new UserPermissionViewModel();
            var checkRole = _dbContext.tbl_Roles.Where(w => w.RoleId == id).FirstOrDefault();
            if (checkRole != null)
            {
                model.RoleId = checkRole.RoleId;
                model.Rolename = checkRole.Rolename;

                var userClaims = _dbContext.tbl_Userclaim.Where(w => w.RoleId == model.RoleId).ToList();

                model.Menus = (from menu in _dbContext.tbl_Menus
                               select new MenuViewModel
                               {
                                   Name = menu.Name + " (" + menu.Parent + ")",
                                   MenuId = menu.MenuId,
                               }).ToList();
                foreach (var item in model.Menus)
                {
                    var checkCliam = userClaims.Where(w => w.MenuId == item.MenuId).FirstOrDefault();
                    if (checkCliam == null)
                    {
                        item.Ischecked = false;
                    }
                    else
                    {
                        item.Ischecked = true;
                    }
                }
            }
            else
            {
                return RedirectToAction("PermissionList");
            }


            return View(model);
        }

        [HttpPost]
        public ActionResult AddPermission(UserPermissionViewModel model)
        {

            if (model.RoleId > 0)
            {
                var existingPermissions = _dbContext.tbl_Userclaim.Where(w => w.RoleId == model.RoleId).ToList();
                _dbContext.tbl_Userclaim.RemoveRange(existingPermissions);
                _dbContext.SaveChanges();

                var newPermissions = model.Menus.Where(w => w.Ischecked).Select(s => new Userclaim
                {
                    MenuId = s.MenuId,
                    CreatedBy = UserId,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    RoleId = model.RoleId
                }).ToList();

                _dbContext.tbl_Userclaim.AddRange(newPermissions);
                _dbContext.SaveChanges();

                ViewBag.SuccessMessage = "Detail added successfully";

            }
            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}