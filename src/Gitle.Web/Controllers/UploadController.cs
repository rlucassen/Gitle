namespace Gitle.Web.Controllers
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Web;
    using Admin;

    public class UploadController : SecureController
    {
        public void File(HttpPostedFile[] uploads)
        {
            var extension = uploads[0].FileName.Substring(uploads[0].FileName.LastIndexOf("."));
            var filename = string.Format("{0}{1}", DateTime.Now.Ticks, extension);
            var path = Path.Combine(ConfigurationManager.AppSettings["fileUpload"], filename);
            var fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
                fileInfo.Delete();
            uploads[0].SaveAs(path);
            RenderText(string.Format("http://gitle.auxilium.nl/Uploads/{0}", filename));
        }
    }
}