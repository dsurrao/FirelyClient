using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace FirelyClient.Pages.Shared.ViewComponents
{
    public class ItemListViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string itemName, List<string> items)
        {

            return View(new Item(itemName, items));
        }
    }

    public struct Item
    {
        public Item(string itemName, List<string> items)
        {
            ItemName = itemName;
            Items = items;
        }

        public string ItemName { get; }
        public List<string> Items { get; }
    }
}
