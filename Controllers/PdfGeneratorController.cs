using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DinkToPdf;
using DinkToPdf.Contracts;
using EmployeeTest.Models;
using EmployeeTest.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EmployeeTest.Controllers
{
    public class PdfGeneratorController : Controller
    {
        private readonly IOptions<Appsettings> _appSettings;
        private readonly DBContext _dbContext;
        private IConverter _converter;

        public PdfGeneratorController(IConverter converter, IOptions<Appsettings> appSettings, DBContext dbContext)
        {
            _converter = converter;
            _appSettings = appSettings;
            _dbContext = dbContext;
        }


        public IActionResult CreatePDF(int id)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdfs",
                "CareGiverTestList" + DateTime.Now.ToString("dd/MM/yyyy-hh-mm-ss") + ".pdf");

            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Landscape,
                PaperSize = PaperKind.A4Plus,
                Margins = new MarginSettings { Top = 10 },
                DocumentTitle = "PDF Report",
                // Out = filePath
            };
            TestUtility _testUtility = new TestUtility(_appSettings, _dbContext);
            var userTests = _testUtility.GetEmployeeTests(id);
            TemplateGeneratorUtility _templateGeneratorUtility = new TemplateGeneratorUtility(_appSettings, _dbContext);

            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = _templateGeneratorUtility.GetHTMLString(userTests),
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "css", "pdfstyles.css") },
                HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
                FooterSettings = { FontName = "Arial", FontSize = 9, Line = true, Center = "" }
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };
            var file = _converter.Convert(pdf);
            return File(file, "application/pdf", "EmployeeReport.pdf");
        }

        public IActionResult CareGiversTest(string id)
        {
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Landscape,
                PaperSize = PaperKind.A4Plus,
                Margins = new MarginSettings { Top = 10 },
                DocumentTitle = "CareGiver Test  Report",
                // Out = filePath
            };

            var userIdList = id.Split(",").ToList();
            userIdList = userIdList.Select(s => "/" + s + "/").ToList();
            var userIds =   String.Join(',', userIdList) ;
            TestUtility _testUtility = new TestUtility(_appSettings, _dbContext);
            TemplateGeneratorUtility _templateGeneratorUtility = new TemplateGeneratorUtility(_appSettings, _dbContext);

            DbfunctionUtility dbfunction = new DbfunctionUtility(_appSettings);
            DataSet ds = dbfunction.GetDataset(@"call get_careGiverTest('" + userIds + "')");
            CommanUtility _commanUtility = new CommanUtility(_appSettings);
            var list = (from row in ds.Tables[0].AsEnumerable()
                        select new PrintPDFViewModel
                        {

                            FirstName = Convert.ToString(row["FirstName"]),
                            MiddleName = Convert.ToString(row["MiddleName"]),
                            LastName = Convert.ToString(row["LastName"]),
                            UserId = Convert.ToInt32(row["UserId"]),
                            Question = Convert.ToString(row["Question"]),
                            Choice1 = Convert.ToString(row["Choice1"]),
                            Choice2 = Convert.ToString(row["Choice2"]),
                            Choice3 = Convert.ToString(row["Choice3"]),
                            Choice4 = Convert.ToString(row["Choice4"]),
                            Choice5 = Convert.ToString(row["Choice5"]),
                            Answer = Convert.ToInt32(row["Answer"]),
                            TestId = Convert.ToInt32(row["testId"]),
                            TestName = Convert.ToString(row["Name"]),
                            UserAnswer = Convert.ToInt16(row["UserSelected"]),
                            EndDate = Convert.ToDateTime(row["EndDate"]),
                            Pass = Convert.ToInt32(row["status"]),
                        }).ToList();


            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = _templateGeneratorUtility.GetHTMLStringForAll(list),
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "css", "pdfstyles.css") },
                HeaderSettings = { FontName = "Arial", FontSize = 9,Center = "StandardPrecautions HIPPA Emergency Preparedness Home Safety", Left = "Print Date: " + DateTime.Now.ToString("MM/dd/yyyy") + "", Right = "Page [page] of [toPage]", Line = false  },
                FooterSettings = { FontName = "Arial", FontSize = 9, Line = false, Center = "" },
               
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };
            var file = _converter.Convert(pdf);
            return File(file, "application/pdf", "CareGiverTestReport.pdf");
        }

    }

}
