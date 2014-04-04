namespace Gitle.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Runtime.Serialization.Json;
    using System.Web;
    using System.Web.Script.Serialization;
    using Admin;
    using Castle.MonoRail.Framework;
    using Helpers;
    using Model;

    public class UploadController : SecureController
    {
        private readonly IList<string> ImageExtensions = new List<string> {".jpg", ".png", ".gif"};
        private readonly IList<string> DocumentExtensions = new List<string> {".doc", ".docx", ".xls", ".xlsx", ".pdf", ".json", ".txt"};

        public void File([FileBinder] IEnumerable<HttpPostedFile> uploads)
        {
            var allowedExtensions = new List<string>();
            allowedExtensions.AddRange(ImageExtensions);
            allowedExtensions.AddRange(DocumentExtensions);
            var feedback = new UploadFeedback();
            foreach (var upload in uploads)
            {
                if (upload.FileName.LastIndexOf(".") == -1)
                {
                    feedback.Errors.Add(upload.FileName, "Bestanden zonder extensie zijn niet toegestaan");
                }
                var extension = upload.FileName.Substring(upload.FileName.LastIndexOf(".")).ToLower();
                if (allowedExtensions.Contains(extension))
                {
                    var info = new FileInfo(upload.FileName);
                    var filename = string.Format("{0}-{1}", DateTime.Now.Ticks, info.Name);
                    var path = Path.Combine(ConfigurationManager.AppSettings["fileUpload"], filename);
                    upload.SaveAs(path);
                    var fileInfo = new FileInfo(path);
                    if (fileInfo.Exists)
                    {
                        var url = filename;
                        if (ImageExtensions.Contains(extension))
                        {
                            url = string.Format("![alt]({0}/Public/{1})",
                                                    ConfigurationManager.AppSettings["webPath"], filename);
                        }
                        else if (DocumentExtensions.Contains(extension))
                        {
                            url = string.Format("[{2}]({0}/Public/{1})",
                                                    ConfigurationManager.AppSettings["webPath"], filename, info.Name);
                        }
                        feedback.Uploads.Add(url);
                    }
                    else
                    {
                        feedback.Errors.Add(upload.FileName, "Er is iets mis gegaan met uploaden");
                    }
                }
                else
                {
                    feedback.Errors.Add(upload.FileName, string.Format("De extensie {0} is niet toegestaan", extension));
                }
            }
            RenderText(new JavaScriptSerializer().Serialize(feedback));
        }
    }
}