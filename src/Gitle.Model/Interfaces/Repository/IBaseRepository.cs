namespace Gitle.Model.Interfaces.Repository
{
    #region Usings

    using System.Collections.Generic;
    using Gitle.Model;
    using Enum;

    #endregion

    public interface IBaseRepository<T> where T : ModelBase
    {
        T Get(long id);
        IList<T> FindAll(FindOption findOption = FindOption.Active);
        bool Save(T item);
        void Delete(T item);
    }
}