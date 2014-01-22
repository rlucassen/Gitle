namespace Gitle.Clients.GitHub.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;
    using Post;

    [DataContract]
    public class Issue
    {
        public Issue()
        {
            Title = string.Empty;
            Labels = new List<Label>();
        }

        private Regex r = new Regex(@"\(.*?\)");
        private Regex repo = new Regex(@"repos/(.*?)/issues");

        [DataMember(Name = "url")]
        public virtual string Url { get; set; }

        public virtual string RepoName
        {
            get { return repo.Matches(Url).Cast<Match>().Select(p => p.Value).FirstOrDefault().Replace("repos/", "").Replace("/issues", ""); }
        }

        [DataMember(Name = "number")]
        public virtual int Number { get; set; }

        [DataMember(Name = "title")]
        public virtual string Title { get; set; }

        [DataMember(Name = "state")]
        public virtual string State { get; set; }

        private string HoursMatch
        {
            get { return r.Matches(Title).Cast<Match>().Select(p => p.Value).LastOrDefault(); }
        }

        public virtual double Hours
        {
            get
            {
                double hours;
                var tryParse = double.TryParse(string.IsNullOrEmpty(HoursMatch) ? "0" : HoursMatch.Trim(new[] {'(', ')'}), out hours);
                return tryParse ? hours : 0;
            }
            set
            {
                Title = string.IsNullOrEmpty(HoursMatch)
                            ? string.Format("{0} ({1})", Name, value)
                            : Title.Replace(HoursMatch, string.Format("({0})", value));
            }
        }

        public virtual string Name
        {
            get { return string.IsNullOrEmpty(HoursMatch) ? Title : Title.Replace(HoursMatch, string.Empty).TrimEnd(' '); }
            set { Title = string.Format("{0} {1}", value, string.IsNullOrEmpty(HoursMatch) ? string.Empty : HoursMatch); }
        }

        [DataMember(Name = "body")]
        public virtual string Body { get; set; }

        [DataMember(Name = "milestone")]
        public virtual Milestone Milestone { get; set; }

        [DataMember(Name = "labels")]
        public virtual List<Label> Labels { get; set; }

        public bool CheckLabel(string label)
        {
            return Labels.Select(l => l.Name).Contains(label);
        }

        [DataMember(Name = "comments")]
        public virtual int Comments { get; set; }

        public virtual IssuePost ToPost()
        {
            return new IssuePost
                       {
                           Title = Title,
                           Body = Body,
                           Milestone = Milestone.Number,
                           Labels = Labels.Select(l => l.Name).ToList()
                       };
        }

        public virtual IssuePatch ToPatch()
        {
            return new IssuePatch
                       {
                           Title = Title,
                           State = State,
                           Body = Body,
                           Milestone = Milestone.Number,
                           Labels = Labels.Select(l => l.Name).ToList()
                       };
        }

    }
}