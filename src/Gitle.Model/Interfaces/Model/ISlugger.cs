namespace Gitle.Model.Interfaces.Model
{
    public interface ISlugger
    {
        string Slug { get; set; }
        bool IsActive { get; }
    }
}