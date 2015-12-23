namespace Gitle.Model.Interfaces.Model
{
    using System.Collections.Generic;

    public interface IDocumentContainer
    {
        IList<Document> Documents { get; set; }
    }
}