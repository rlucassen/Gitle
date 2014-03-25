namespace Gitle.Model.Interfaces.Repository
{
    using System.Collections.Generic;
    using Enum;

    public interface ILabelRepository : IBaseRepository<Label>
    {
        Label FindByName(string name);
    }
}