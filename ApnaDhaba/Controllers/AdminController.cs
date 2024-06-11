using ApnaDhaba.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ApnaDhaba.Controllers
{
    //[Authorize(Roles = "ADMIN")]
    public class AdminController : Controller
    {
        private readonly FoodieDbContext xyzcontext;

        public AdminController(FoodieDbContext xyzcontext)
        {
            this.xyzcontext = xyzcontext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Category()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Category(Category cate)
        {
            if (cate != null)
            {
                xyzcontext.Categories.Add(cate);
                xyzcontext.SaveChanges();

                TempData["addedSuccess"] = "Success!Category Addded";
            }

            return View();
        }

        public IActionResult Product()
        {
            dualModel m = new dualModel();

            m.dualcatList = CategoryList();

            return View(m);
        }

        [HttpPost]
        public IActionResult Product(dualModel m)
        {
            dualModel dualcat = new dualModel();
            dualcat.dualcatList = CategoryList();

            xyzcontext.Products.Add(m.dualProduct);
            xyzcontext.SaveChanges();

            TempData["addedSuccess"] = "Product Added";
            return View(dualcat);
        }

        private Cat_List CategoryList()
        {
            Cat_List ft = new Cat_List();

            //accesssing List Property of that model and for appending/populating data created a instance
            ft.catList = new List<SelectListItem>();

            //data source converted into list
            var data = xyzcontext.Categories.ToList();

            //checking if data is not empty and creating default view with empty value
            if (data != null)
            {
                ft.catList.Add(new SelectListItem
                {
                    Value = "",
                    Text = "Select Name"
                });

                //for SelectListItem Value and Text are property that sets the value in <option>
                foreach (var item in data)
                {
                    if (data != null)
                    {
                        ft.catList.Add(new SelectListItem
                        {
                            Value = item.CategoryId.ToString(),
                            Text = item.Name
                        });
                    }
                }
            }
            //passing the object to view
            return ft;
        }
    }
}