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
    public class DocumentController : Controller
    {
        private readonly IOptions<Appsettings> _appSettings;
        private readonly DBContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession _session => _httpContextAccessor.HttpContext.Session;
        private int UserId = 0;

        public DocumentController(IOptions<Appsettings> appSettings, DBContext dbContext, IHttpContextAccessor HttpContextAccessor)
        {
            _appSettings = appSettings;
            _dbContext = dbContext;
            _httpContextAccessor = HttpContextAccessor;
            int.TryParse(_session.GetString("UserId"), out UserId);
        }

        public IActionResult List()
        {
            var documents = new List<DocumentViewModel>();
            try
            {

                documents = (from document in _dbContext.tbl_Documents
                             join season in _dbContext.tbl_Seasons
                             on document.SeasonId equals season.Id
                             into season
                             from season1 in season.DefaultIfEmpty()
                             
                             select new DocumentViewModel
                             {
                                 DocumentId = document.DocumentId,
                                 Name = document.Name,
                                 IsActive = document.IsActive,
                                 CreatedDate = document.CreatedDate,
                                 Description = document.Description,
                                 SeasonId = document.SeasonId??0,
                                 SeasonName = season1.Name

                             }).ToList();

            }
            catch (Exception ex)
            {

            }
            return View(documents);
        }

        public ActionResult Add()
        {
            DocumentViewModel model = new DocumentViewModel();
            model.SeasonList = _dbContext.tbl_Seasons.Where(w => w.IsActive == true)
                          .Select(s => new SeasonViewModel
                          {
                              Name = s.Name,
                              Id = s.Id
                          }).ToList();
            return View(model);
        }

        public ActionResult Edit(string id)
        {
            DocumentViewModel model = new DocumentViewModel();
            try
            {

                {
                    DbfunctionUtility dbfunction = new DbfunctionUtility(_appSettings);
                    DataSet ds = dbfunction.GetDataset("select * from documents where documentId= '" + id + "'");
                    int idInt = 0;
                    int.TryParse(id, out idInt);
                    model = _dbContext.tbl_Documents.Where(w => w.DocumentId == idInt)
                        .Select(s => new DocumentViewModel
                        {
                            DocumentId = s.DocumentId,
                            Name = s.Name,
                            Description = s.Description,
                            SeasonId = s.SeasonId??0

                        }).FirstOrDefault();
                    model.SeasonList = _dbContext.tbl_Seasons.Where(w => w.IsActive == true)
                       .Select(s => new SeasonViewModel
                       {
                           Name = s.Name,
                           Id = s.Id
                       }).ToList();

                    if (model != null)
                    {
                        return View(model);
                    }
                    else
                    {
                        return RedirectToAction("List", "Question");
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return View(model);

        }

        [HttpPost]
        public ActionResult Add(DocumentViewModel model, IFormFile file)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    DbfunctionUtility dbfunction = new DbfunctionUtility(_appSettings);
                    var query = "select * from Documents where Name = '" + model.Name + "' and seasonId = "+model.SeasonId+"";
                    DataSet ds = dbfunction.GetDataset(query);
                    if (ds.Tables[0].Rows.Count == 0)
                    {

                        var document = new Document
                        {
                            Name = model.Name,
                            IsActive = true,
                            CreatedBy = UserId,
                            CreatedDate = DateTime.Now,
                            FileName = file.FileName,
                            Description = model.Description,
                            SeasonId = model.SeasonId
                        };

                        _dbContext.tbl_Documents.Add(document);
                        _dbContext.SaveChanges();

                        var fileName = document.DocumentId.ToString() + Path.GetExtension(file.FileName);
                        document.FileName = fileName;
                        _dbContext.SaveChanges();
                        DocumentUtility _documentUtility = new DocumentUtility();
                        _documentUtility.SaveFile(document.DocumentId.ToString(), "Document", file);

                        ViewBag.SuccessMessage = "File added successfully";
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "File is already exists";
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(DocumentViewModel model, IFormFile file)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    DbfunctionUtility dbfunction = new DbfunctionUtility(_appSettings);
                    var query = "select * from Documents where Name = '" + model.Name + "' and DocumentId !='" + model.DocumentId + "' and seasonId = " + model.SeasonId + " ";
                    DataSet ds = dbfunction.GetDataset(query);
                    if (ds.Tables[0].Rows.Count == 0)
                    {
                        var fileName = model.DocumentId.ToString() + Path.GetExtension(file.FileName);
                        var document = _dbContext.tbl_Documents.Where(w => w.DocumentId == model.DocumentId).FirstOrDefault();
                        document.Name = model.Name;
                        document.FileName = fileName;
                        document.ModifiedBy = UserId;
                        document.ModifiedDate = DateTime.Now;
                        document.Description = model.Description;
                        document.SeasonId = model.SeasonId;
                        _dbContext.SaveChanges();
                        DocumentUtility _documentUtility = new DocumentUtility();
                        _documentUtility.SaveFile(document.DocumentId.ToString(), "Document", file);
                        ViewBag.SuccessMessage = "File updated successfully";
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "File is already exists";
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
                int idInt = 0;
                int.TryParse(id, out idInt);
                QuestionViewModel model = new QuestionViewModel();
                var document = _dbContext.tbl_Documents.Where(w => w.DocumentId == idInt).FirstOrDefault();
                document.IsActive = false;
                document.ModifiedBy = UserId;
                document.ModifiedDate = DateTime.Now;
                _dbContext.SaveChanges();
                response.Status = "1";
                response.Message = "File deleted successfully";
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