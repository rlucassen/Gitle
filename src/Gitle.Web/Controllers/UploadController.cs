namespace Gitle.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Web;
    using System.Web.Script.Serialization;
    using Clients.Freckle.Interfaces;
    using Helpers;
    using Model;
    using Model.Interfaces.Model;
    using NHibernate;

    public class UploadController : SecureController
    {
        private static readonly IList<string> ImageExtensions = new List<string> {".jpg", ".png", ".gif"};
        private static readonly IList<string> DocumentExtensions = new List<string> {".doc", ".docx", ".xls", ".xlsx", ".pdf", ".json", ".txt", ".xml", ".xsd", ".msg", ".apk"};

        public UploadController(ISessionFactory sessionFactory) : base(sessionFactory)
        {
        }

        public void File([FileBinder] IEnumerable<HttpPostedFile> uploads)
        {
            var allowedExtensions = new List<string>();
            allowedExtensions.AddRange(ImageExtensions);
            allowedExtensions.AddRange(DocumentExtensions);
            var feedback = new UploadFeedback();
            foreach (var upload in uploads)
            {
                var extension = Path.GetExtension(upload.FileName).ToLower();
                if (string.IsNullOrEmpty(extension))
                {
                    feedback.Errors.Add(upload.FileName, "Bestanden zonder extensie zijn niet toegestaan");
                }
                else if (!allowedExtensions.Contains(extension))
                {
                    feedback.Errors.Add(upload.FileName, $"De extensie {extension} is niet toegestaan");
                }
                else
                {
                    var info = new FileInfo(upload.FileName);
                    var filename = $"{DateTime.Now.Ticks}-{info.Name}";
                    var path = Path.Combine(ConfigurationManager.AppSettings["fileUpload"], filename);
                    upload.SaveAs(path);
                    var fileInfo = new FileInfo(path);
                    if (fileInfo.Exists)
                    {
                        var url = filename;
                        var webPath = ConfigurationManager.AppSettings["webPath"];
                        if (ImageExtensions.Contains(extension))
                        {
                            url = $"![alt]({webPath}/Public/{filename})";
                        }
                        else if (DocumentExtensions.Contains(extension))
                        {
                            url = $"[{info.Name}]({webPath}/Public/{filename})";
                        }
                        feedback.Uploads.Add(url);
                    }
                    else
                    {
                        feedback.Errors.Add(upload.FileName, "Er is iets mis gegaan met uploaden");
                    }
                }
                }
            RenderText(new JavaScriptSerializer().Serialize(feedback));
        }

        private void Document<T>(IEnumerable<HttpPostedFile> uploads, long id) where T : IDocumentContainer
        {
            var item = session.Get<T>(id);
            var allowedExtensions = new List<string>();
            allowedExtensions.AddRange(ImageExtensions);
            allowedExtensions.AddRange(DocumentExtensions);
            var feedback = new DocumentUploadFeedback();
            using (var tx = session.BeginTransaction())
            {
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
                            var url = string.Format("/Public/{0}", filename);
                            var document = new Document() { Name = info.Name, DateUploaded = DateTime.Now, Path = url, User = CurrentUser };

                            item.Documents.Add(document);
                            session.SaveOrUpdate(document);

                            feedback.Uploads.Add(document);
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
                session.SaveOrUpdate(item);
                tx.Commit();
            }
            RenderText(new JavaScriptSerializer().Serialize(feedback));
        }

        public void ProjectDocument([FileBinder] IEnumerable<HttpPostedFile> uploads, long id)
        {
            Document<Project>(uploads, id);
        }
    }
}