namespace Gitle.Model
{
    public class UserProject : ModelBase
    {
        public virtual Project Project { get; set; }
        public virtual User User { get; set; }
        public virtual bool Notifications { get; set; }
        public virtual bool OnlyOwnIssues { get; set; }
        public virtual bool ConfirmOwnEntries { get; set; }

        public virtual bool Subscribed { get; set; }
    }
}