namespace Gitle.Web.Helpers
{
    using System.Collections.Generic;

    public class UploadFeedback
    {
        public UploadFeedback()
        {
            Uploads = new List<string>();
            Errors = new Dictionary<string, string>();
        }

        public IList<string> Uploads { get; set; }
        public Dictionary<string, string> Errors { get; set; } 
    }
}