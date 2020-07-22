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
    public class RoleController : Controller
    {
        private readonly IOptions<Appsettings> _appSettings;
        private readonly DBContext _dbContext;

        public RoleController(IOptions<Appsettings> appSettings, DBContext dbContext)
        {
            _appSettings = appSettings;
            _dbContext = dbContext;

        }

        public IActionResult List()
        {
            var roles = new List<RoleViewModel>();
            try
            {

                roles = (from role in _dbContext.tbl_Roles
                         select new RoleViewModel
                         {
                             RoleId = role.RoleId,
                             Rolename = role.Rolename
                         }).ToList();

            }
            catch (Exception ex)
            {

            }
            return View(roles);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}