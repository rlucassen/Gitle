namespace Gitle.Model
{
    using System;

    public class Comment : ModelBase
    {
        public virtual string Text { get; set; }
        public virtual DateTime CreatedAt { get; set; }

        public virtual User User { get; set; }
        public virtual Issue Issue { get; set; }

        public virtual string Name { get { return User != null ? User.FullName : "Auxilium"; } }
    }
}