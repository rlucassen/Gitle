namespace Gitle.Clients.GitHub.Models
{
    using System;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;
    using Post;

    [DataContract]
    public class Comment
    {
        private Regex r = new Regex(@"\(.*?\): ");

        [DataMember(Name = "id")]
        public virtual int Id { get; set; }

        [DataMember(Name = "body")]
        public virtual string Body { get; set; }

        public virtual string Name
        {
            get
            {
                var match = r.Matches(Body).Cast<Match>().Select(p => p.Value).FirstOrDefault();
                return string.IsNullOrEmpty(match) ? User.Login : match.Replace("): ", "").TrimStart('(');
            }
        }

        public virtual string Text
        {
            get
            {
                return Body.Replace(string.Format("({0}): ", Name), "");
            }
        }

        [DataMember(Name = "user")]
        public virtual User User { get; set; }

        [DataMember(Name = "created_at")]
        public virtual DateTime CreatedAt { get; set; }

        public virtual CommentPost ToPost()
        {
            return new CommentPost
                       {
                           Body = Body
                       };
        }
    }
}