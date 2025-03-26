using Microsoft.AspNetCore.Mvc;
using System.Reflection.PortableExecutable;
using System.Text.Json;

namespace Website.Components
{
    public class ListWidgetViewComponent : ViewComponent
    {

        public ListWidgetViewComponent()
        {
        }

        public async Task<IViewComponentResult> InvokeAsync(string title, string apiUrl, string redirectUrl)
        {
            ViewData["ApiUrl"] = apiUrl;
            ViewData["Title"] = title;
            ViewData["RedirectUrl"] = redirectUrl;
            return View();
        }
    }
}
