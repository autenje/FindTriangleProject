using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using System.IO;
using FindTriangles.Models;

namespace FindTriangles.Controllers
{
  
    public class FileController : ApiController
    {
            [HttpPost]
         public void UploadFile()
         {
                
             if (HttpContext.Current.Request.Files.AllKeys.Any())
             {
                 // Delete folder contents prior to upload

                 DeleteUploadedFiles();

                 // Get the uploaded image from the Files collection
                 var httpPostedFile = HttpContext.Current.Request.Files["UploadedImage"];

                 if (httpPostedFile != null)
                 {
                     // Validate the uploaded image(optional)

                     // Get the complete file path
                     var fileSavePath = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles"), httpPostedFile.FileName);

                     // Save the uploaded file to "UploadedFiles" folder
                     httpPostedFile.SaveAs(fileSavePath);
                 }
             }
         }

            private void DeleteUploadedFiles()
            {
                String sourceDir = System.Web.Hosting.HostingEnvironment.MapPath(@"~/UploadedFiles"); ;
                string[] filePaths = System.IO.Directory.GetFiles(sourceDir);
                foreach (string filePath in filePaths)
                    File.Delete(filePath);


            }
    }
}
