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
        private readonly IList<string> AllowedExtensions = new List<string> {".jpg", ".png", ".gif", ".JPG", ".PNG", ".GIF"};

        public void File([FileBinder] IEnumerable<HttpPostedFile> uploads)
        {
            var feedback = new UploadFeedback();
            foreach (var upload in uploads)
            {
                var extension = upload.FileName.Substring(upload.FileName.LastIndexOf("."));
                if (AllowedExtensions.Contains(extension))
                {
                    var info = new FileInfo(upload.FileName);
                    var filename = string.Format("{0}-{1}", DateTime.Now.Ticks, info.Name);
                    var path = Path.Combine(ConfigurationManager.AppSettings["fileUpload"], filename);
                    upload.SaveAs(path);
                    var fileInfo = new FileInfo(path);
                    if (fileInfo.Exists)
                    {
                        var url = string.Format("{0}/Public/{1}", ConfigurationManager.AppSettings["webPath"], filename);
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