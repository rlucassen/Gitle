namespace Gitle.Web.Helpers
{
    using System.Collections.Generic;
    using Model;

    public class SaveFeedback
    {
        public bool Success { get; set; }
        public Dictionary<string, string> Errors { get; set; }
        public string Type { get; set; }
        public long Id { get; set; }
        public string Action { get; set; }

        public SaveFeedback(ModelBase instance)
        {
            Success = instance.IsValid();
            Errors = instance.InvalidValues();
            Type = NHibernate.NHibernateUtil.GetClass(instance).Name;
            Id = instance.Id;
            Action = "index";
        }

        public SaveFeedback(ModelBase instance, bool success)
        {
            Success = success;
            Errors = instance.InvalidValues();
            Type = NHibernate.NHibernateUtil.GetClass(instance).Name;
            Id = instance.Id;
            Action = "index";
        }
    }
}