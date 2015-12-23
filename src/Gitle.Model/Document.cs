namespace Gitle.Model
{
    using System;
    using System.Dynamic;
    using System.Web.Script.Serialization;
    using Helpers;
    using Newtonsoft.Json;

    public class Document : ModelBase
    {
        public virtual string Name { get; set; }
        public virtual string Path { get; set; }
        public virtual DateTime DateUploaded { get; set; }
        [ScriptIgnore]
        public virtual User User { get; set; }
        public virtual string DateString => DateUploaded.Readable();
    }
}