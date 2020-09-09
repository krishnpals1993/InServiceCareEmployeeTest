﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using EmployeeTest.Models;
using EmployeeTest.Utility;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Linq;

namespace EmployeeTest.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        private readonly IOptions<Appsettings> _appSettings;
        private readonly IOptions<EmailSettings> _emailSettings;
        private readonly DBContext _dbContext;

        public AccountController(IOptions<Appsettings> appSettings, IOptions<EmailSettings> emailSettings, DBContext dbContext)
        {
            _appSettings = appSettings;
            _emailSettings = emailSettings;
            _dbContext = dbContext;
        }

        public ActionResult Login()
        {
            HttpContext.Session.SetString("UserId", "");
            HttpContext.Session.SetString("RoleId", "");
            HttpContext.Session.SetString("Username", "");
            HttpContext.Session.SetString("UserMenus", "");
            HttpContext.Session.SetString("HrGroupId", "");
            return View();
        }



        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    DbfunctionUtility dbfunction = new DbfunctionUtility(_appSettings);
                    DataSet ds = dbfunction.GetDataset("select * from users join roles on users.roleid = roles.roleid  where username ='" + model.Username + "' and Password ='" + model.Password + "'");

                    if (ds.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.ErrorMessage = "Incorrect username or password";
                    }
                    else
                    {
                        CommanUtility commanUtility = new CommanUtility(_appSettings);
                        var userMenus = commanUtility.GetUserMenus(Convert.ToString(ds.Tables[0].Rows[0]["RoleId"]));
                        HttpContext.Session.SetString("UserMenus", JsonConvert.SerializeObject(userMenus));
                        HttpContext.Session.SetString("UserId", Convert.ToString(ds.Tables[0].Rows[0]["Userid"]));
                        HttpContext.Session.SetString("RoleName", Convert.ToString(ds.Tables[0].Rows[0]["RoleName"]));
                        HttpContext.Session.SetString("HrGroupId", Convert.ToString(ds.Tables[0].Rows[0]["HrGroupId"]));
                        HttpContext.Session.SetString("Username", model.Username);
                        if (Convert.ToString(ds.Tables[0].Rows[0]["RoleName"]) != "CareGiver")
                        {
                            return RedirectToAction("List", "CareGiver");
                        }
                        else
                        {
                            var TestList = _dbContext.tbl_Tests.Where(w => w.SeasonId == 1).ToList();
                            var testCount = 0;

                            foreach (var test in TestList)
                            {
                                var totalVideos = _dbContext.tbl_Testvideos.Count();
                                var watchedVideos = (from userVideo in _dbContext.tbl_AttendentTestVideos
                                                            join testVideo in _dbContext.tbl_Testvideos
                                                            on userVideo.VideoId equals testVideo.Id
                                                            where userVideo.UserId == Convert.ToInt32(ds.Tables[0].Rows[0]["Userid"])
                                                            && testVideo.TestId == test.Id && userVideo.IsCompleted == true
                                                     select testVideo.Id).Count();

                                if (totalVideos == watchedVideos)
                                {
                                    testCount++;
                                }
                            }

                            if (testCount > 0 )
                            {
                                HttpContext.Session.SetString("ShowTest", "True");
                                return RedirectToAction("Exam", "Attendant");
                            }
                            else
                            {
                                HttpContext.Session.SetString("ShowTest", "False");
                                HttpContext.Session.SetString("Username", model.Username);
                                return RedirectToAction("Videos", "Test");
                            }

                        }

                    }
                }
            }
            catch (Exception ex)
            {

            }

            return View(model);
        }

        public ActionResult Register()
        {
            RegisterViewModel model = new RegisterViewModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Register(RegisterViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    var roleId = _dbContext.tbl_Roles.Where(w => w.Rolename == "CareGiver").FirstOrDefault().RoleId;
                    var checkUser = _dbContext.tbl_Users.Where(w => w.Email == model.Email).FirstOrDefault();
                    if (checkUser == null)
                    {
                        User user = new User
                        {
                            Username = model.Email,
                            Password = model.Password,
                            Email = model.Email,
                            IsActive = true,
                            RoleId = roleId,
                            CreatedDate = DateTime.Now,

                        };
                        _dbContext.tbl_Users.Add(user);
                        _dbContext.SaveChanges();

                        var attendatDetail = _dbContext.tbl_Attendants.Where(w => w.Email == model.Email).FirstOrDefault();
                        if (attendatDetail != null)
                        {
                            attendatDetail.UserId = user.UserId;
                        }
                        _dbContext.SaveChanges();

                        ViewBag.SuccessMessage = "Registration successfully";
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Employee already exist for this email";
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "Please enter valid detail";
                }
            }
            catch (Exception ex)
            {

            
            }
            

            return View(model);
        }

        public ActionResult ForgotPassword()
        {
            ForgetPasswordViewModel model = new ForgetPasswordViewModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult ForgotPassword(ForgetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                EmailUtility _emaUtility = new EmailUtility(_appSettings, _emailSettings);
                CommanUtility _commanUtility = new CommanUtility(_appSettings);
                var encEmail = _commanUtility.EncryptString(model.Email);
                string emailBody = @"<p>Please click on  following link to reset password</p>
                                    <br />
                                  <p><a href='" + _appSettings.Value.WebBaseURL + "/Account/ResetPassword?email=" + encEmail + "'>" + _appSettings.Value.WebBaseURL + "/ResetPassword?email=" + encEmail + "</a></p>";


                _emaUtility.SendEmail("", "Reset Password link for employee test", emailBody, model.Email.Split(","));

            }
            else
            {

            }

            return View(model);
        }

        public ActionResult ResetPassword(string email)
        {
            ResetPasswordViewModel model = new ResetPasswordViewModel();
            CommanUtility _commanUtility = new CommanUtility(_appSettings);
            try
            {
                model.Email = _commanUtility.DecryptString(email);

            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Email not found ";

            }

            return View(model);
        }

        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {

                var checkUser = _dbContext.tbl_Users.Where(w => w.Email == model.Email).FirstOrDefault();
                if (checkUser != null)
                {
                    checkUser.Password = model.Password;
                    _dbContext.SaveChanges();
                    ViewBag.SuccessMessage = "Password reset successfully";
                }
                else
                {
                    ViewBag.ErrorMessage = "Email not exists";
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Please enter valid detail";
            }

            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}