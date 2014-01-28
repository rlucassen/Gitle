namespace Gitle.Web.Controllers
{
    using Model.Helpers;

    public class SlugController : BaseController
    {
         public void Index(string text)
         {
             CancelLayout();
             RenderText(string.IsNullOrEmpty(text) ? string.Empty : text.Slugify());
         }
    }
}