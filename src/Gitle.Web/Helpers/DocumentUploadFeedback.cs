namespace Gitle.Web.Helpers
{
    using System.Collections.Generic;
    using Model;

    public class DocumentUploadFeedback
    {
        public DocumentUploadFeedback()
        {
            Uploads = new List<Document>();
            Errors = new Dictionary<string, string>();
        }

        public IList<Document> Uploads { get; set; }
        public Dictionary<string, string> Errors { get; set; } 
    }
}