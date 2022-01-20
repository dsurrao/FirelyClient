using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace FirelyClient.Pages.Shared.ViewComponents
{
    public class ItemListViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var items = new List<string>() { "one", "two", "three" };
            return View(items);
        }
    }
}
