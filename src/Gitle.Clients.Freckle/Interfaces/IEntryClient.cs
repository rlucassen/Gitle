namespace Gitle.Clients.Freckle.Interfaces
{
    using System.Collections.Generic;
    using Models;

    public interface IEntryClient
    {
        bool Post(Entry entry);
    }
}