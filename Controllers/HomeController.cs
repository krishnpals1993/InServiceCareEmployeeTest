﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using EmployeeTest.Filters;
using EmployeeTest.Models;
using EmployeeTest.Utility;
using System;
using System.Data;
using System.Linq;

namespace EmployeeTest.Controllers
{
    //[Authentication]
    public class HomeController : Controller
    {
        private readonly IOptions<Appsettings> _appSettings;
        private readonly DBContext _dbContext;

        public HomeController(IOptions<Appsettings> appSettings, DBContext dbContext)
        {
            _appSettings = appSettings;
            _dbContext = dbContext;
        }

        public ActionResult Dashboard()
        {
           
            try
            {

                DbfunctionUtility dbfunction = new DbfunctionUtility(_appSettings);
                DataSet ds = new DataSet();


            }
            catch (Exception ex)
            {


            }

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}