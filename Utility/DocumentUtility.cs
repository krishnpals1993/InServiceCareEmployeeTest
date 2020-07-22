using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeTest.Utility
{
    public class DocumentUtility
    {

        public void SaveFile(string id, string folderName, Microsoft.AspNetCore.Http.IFormFile file)
        {
            try
            {
                var fileName = id.ToString() + Path.GetExtension(file.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderName, fileName);
                using (var stream = new FileStream(path, FileMode.Create)) { file.CopyTo(stream); }
            }
            catch (Exception ex)
            {


            }


        }

        public void checkFolder(string folderName)
        {
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderName)))
            {
                File.Create(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderName));
            }
        }

        public string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},  
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"}
            };
        }

    }
}
