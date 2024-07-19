using ApnaDhaba.Api_Models;
using ApnaDhaba.Models;
using ApnaDhaba.Models.Other;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace ApnaDhaba.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly FoodieDbContext dbContext;
        private HttpClient client = new HttpClient();
        private string urlFetch = "https://localhost:7215/api/Auth/GetDetails";
        private string urlUpdate = "https://localhost:7215/api/Auth/UpdateUser";
        private string urlFetchAddress = "https://localhost:7215/api/Home/GetAddress";

        public HomeController(FoodieDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IActionResult Index()
        {
            dualModel dual = new dualModel();

            var data = dbContext.Products.ToList();
            var cat = dbContext.Categories.ToList();

            dual.dualProductList = data;
            dual.dualCatList = cat;
            //create list
            if (HttpContext.Session.GetString("AuthUser") == null && HttpContext.Session.GetString("Guest") == null)
            {
                Random random = new Random();
                string guest = "Active Guest #";
                int guestID = random.Next(1, 5000);
                string NewUser = guest + guestID;
                HttpContext.Session.SetString("BasketID", Convert.ToString(guestID));
                var exists = dbContext.Carts.Where(x => x.Username == NewUser).ToList();
                if (exists != null)
                {
                    do
                    {
                        int newGuestID = random.Next(1, 5000);
                        HttpContext.Session.Remove("BasketID");
                        HttpContext.Session.SetString("BasketID", Convert.ToString(newGuestID));
                        NewUser = guest + newGuestID;
                    } while (exists == null);
                }

                HttpContext.Session.SetString("Guest", NewUser);
                SessionOptions options = new SessionOptions();
                options.IdleTimeout = TimeSpan.FromMinutes(2);

                TempData["Guest"] = "Guest";
            }
            else
            {
                ViewData["AuthUser"] = HttpContext.Session.GetString("AuthUser");
            }

            if (HttpContext.Session.GetString("PaymentDone") != null)
            {
                string val = HttpContext.Session.GetString("PaymentDone");

                TempData["Payment"] = val;
                HttpContext.Session.Remove("PaymentDone");
            }

            return View(dual);
        }

        public IActionResult Menu()
        {
            dualModel dual = new dualModel();

            var data = dbContext.Products.ToList();
            var cat = dbContext.Categories.ToList();

            dual.dualProductList = data;
            dual.dualCatList = cat;
            //create list
            Calculate();
            return View(dual);
        }

        [HttpPost]
        public IActionResult Menu(dualModel d)
        {
            if (HttpContext.Session.GetString("AuthUser") == null)
            {
                HttpContext.Session.GetString("Guest");

                var MenuGuestdata = FeedMenuData(d);

                return View(MenuGuestdata);
            }
            else
            {
                var Menudata = FeedMenuData(d);

                return View(Menudata);
            }
        }

        public IActionResult Cart()
        {
            if (HttpContext.Session.GetString("AuthUser") == null)
            {
                var guest = HttpContext.Session.GetString("Guest");
                Calculate();
                MultiModel multiModel = new MultiModel();

                List<Cart> carts = dbContext.Carts.Where(x => x.Username == guest).ToList();
                List<Product> prods = new List<Product>();

                foreach (var i in carts)
                {
                    var data = dbContext.Products.FirstOrDefault(x => x.ProductId == i.ProductId);

                    prods.Add(data);
                }

                multiModel.cart = carts;
                multiModel.Products = prods;

                if (HttpContext.Session.GetString("SubTotal") != null && HttpContext.Session.GetString("Total") != null)
                {
                    ViewBag.Subtotal = HttpContext.Session.GetString("SubTotal");
                    ViewBag.Total = HttpContext.Session.GetString("Total");
                    if (ViewBag.Subtotal == "0")
                    {
                        ViewBag.CartEmpty = "True";
                    }
                }
                if (HttpContext.Session.GetString("ActiveCart") != null)
                {
                    ViewBag.ActiveCart = HttpContext.Session.GetString("ActiveCart");
                }
                return View(multiModel);
            }
            else
            {
                if (HttpContext.Session.GetString("UpdateUserGuest") != null)
                {
                    var oldguestUser = HttpContext.Session.GetString("Guest");
                    var newUpdateGuest = HttpContext.Session.GetString("UpdateUserGuest");
                    var updateCartUser = dbContext.Carts.Where(x => x.Username == oldguestUser).ToList();

                    foreach (var item in updateCartUser)
                    {
                        item.Username = newUpdateGuest;
                    }
                    dbContext.Carts.UpdateRange(updateCartUser);
                    dbContext.SaveChanges();
                    HttpContext.Session.Remove("UpdateUserGuest");
                    HttpContext.Session.SetString("Checkout", newUpdateGuest);
                    return RedirectToAction("Payment");
                }

                var user = HttpContext.Session.GetString("AuthUser");
                Calculate();
                MultiModel multiModel = new MultiModel();

                List<Cart> carts = dbContext.Carts.Where(x => x.Username == user).ToList();
                List<Product> prods = new List<Product>();

                foreach (var i in carts)
                {
                    var data = dbContext.Products.FirstOrDefault(x => x.ProductId == i.ProductId);

                    prods.Add(data);
                }
                multiModel.cart = carts;
                multiModel.Products = prods;

                if (HttpContext.Session.GetString("SubTotal") != null && HttpContext.Session.GetString("Total") != null)
                {
                    ViewBag.Subtotal = HttpContext.Session.GetString("SubTotal");
                    ViewBag.Total = HttpContext.Session.GetString("Total");
                    if (ViewBag.Subtotal == "0")
                    {
                        ViewBag.CartEmpty = "True";
                    }
                }
                if (HttpContext.Session.GetString("ActiveCart") != null)
                {
                    ViewBag.ActiveCart = HttpContext.Session.GetString("ActiveCart");
                }
                return View(multiModel);
            }
        }


        [HttpPost]
        public IActionResult min(int id, decimal quant)
        {
            if (HttpContext.Session.GetString("AuthUser") != null)
            {
                var user = HttpContext.Session.GetString("AuthUser");
                var data = dbContext.Carts.FirstOrDefault(x => x.ProductId == id && x.Username == user);
                var productPrice = dbContext.Products.FirstOrDefault(x => x.ProductId == data.ProductId);
                if (data.Quantity >= 1 && data.Quantity <= 10)
                {
                    data.Quantity = quant - 1;
                    data.TotalPrice = data.Quantity * productPrice.Price;

                    dbContext.Carts.Update(data);
                    dbContext.SaveChanges();
                }
                if (data.Quantity == 0)
                {
                    dbContext.Carts.Remove(data);
                    dbContext.SaveChanges();
                    TempData["Limit"] = "Item Removed From The Cart";
                }

                Calculate();
                return RedirectToAction("Cart");
            }
            else if (HttpContext.Session.GetString("Guest") != null)
            {
                var user = HttpContext.Session.GetString("Guest");
                var data = dbContext.Carts.FirstOrDefault(x => x.ProductId == id && x.Username == user);
                var productPrice = dbContext.Products.FirstOrDefault(x => x.ProductId == data.ProductId);
                if (data.Quantity >= 1 && data.Quantity <= 10)
                {
                    data.Quantity = quant - 1;
                    data.TotalPrice = data.Quantity * productPrice.Price;

                    dbContext.Carts.Update(data);
                    dbContext.SaveChanges();
                }
                if (data.Quantity == 0)
                {
                    dbContext.Carts.Remove(data);
                    dbContext.SaveChanges();
                    TempData["Limit"] = "Item Removed From The Cart";
                }

                Calculate();
                return RedirectToAction("Cart");
            }
            else return RedirectToAction("_NotFound", "Auth");
        }

        [HttpPost]
        public IActionResult plus(int id, decimal quant)
        {
            if (HttpContext.Session.GetString("AuthUser") != null)
            {
                var user = HttpContext.Session.GetString("AuthUser");
                var data = dbContext.Carts.FirstOrDefault(x => x.ProductId == id && x.Username == user);
                var productPrice = dbContext.Products.FirstOrDefault(x => x.ProductId == data.ProductId);

                if (data.Quantity >= 1 && data.Quantity < 10)
                {
                    data.Quantity = quant + 1;
                    data.TotalPrice = data.Quantity * productPrice.Price;

                    dbContext.Carts.Update(data);
                    dbContext.SaveChanges();
                }
                if (data.Quantity == 10)
                {
                    TempData["Limit"] = "Max Item Order Limit Reached";
                }
                Calculate();
                return RedirectToAction("Cart");
            }
            else if (HttpContext.Session.GetString("Guest") != null)
            {
                var user = HttpContext.Session.GetString("Guest");
                var data = dbContext.Carts.FirstOrDefault(x => x.ProductId == id && x.Username == user);
                var productPrice = dbContext.Products.FirstOrDefault(x => x.ProductId == data.ProductId);

                if (data.Quantity >= 1 && data.Quantity < 10)
                {
                    data.Quantity = quant + 1;
                    data.TotalPrice = data.Quantity * productPrice.Price;

                    dbContext.Carts.Update(data);
                    dbContext.SaveChanges();
                }
                if (data.Quantity == 10)
                {
                    TempData["Limit"] = "Max Item Order Limit Reached";
                }
                Calculate();
                return RedirectToAction("Cart");
            }
            else return RedirectToAction("_NotFound", "Auth");
        }

        public IActionResult UserProfile()
        {
            if (HttpContext.Session.GetString("AuthUser") != null)
            {
                ViewData["username"] = HttpContext.Session.GetString("AuthUser");

                return View();
            }
            else
            {
                return RedirectToAction("Login", "Auth");
            }
        }

        [HttpPost]
        public IActionResult UserProfile(UserModel user)
        {
            if (HttpContext.Session.GetString("AuthUser") != null)
            {
                if (user != null)
                {
                    string data = JsonConvert.SerializeObject(user);

                    StringContent content = new StringContent(data, Encoding.UTF8, "application/JSON");

                    HttpResponseMessage response = client.PostAsync(urlFetch, content).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["done"] = "true";

                        string read = response.Content.ReadAsStringAsync().Result;

                        var data2 = JsonConvert.DeserializeObject<UserModel>(read);

                        return View(data2);
                    }
                    else
                    {
                        return View();
                    }
                }

                return View();
            }
            else
            {
                TempData["SessionEnded"] = "Session Ended ! !";
                return RedirectToAction("SessionEnded", "Auth");
            }
        }

        [HttpPost]
        public IActionResult Update(UserModel user)
        {
            if (user != null)
            {
                string data = JsonConvert.SerializeObject(user);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/JSON");
                HttpResponseMessage response = client.PutAsync(urlUpdate, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    TempData["update"] = "User Updated Successfully";
                    return RedirectToAction("UserProfile");
                }
                else
                {
                    TempData["updatedFailed"] = "User Update Failed";

                    return RedirectToAction("UserProfile");
                }
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet]
        public IActionResult RCare(int? id)
        {
            var latestDataList = dbContext.Carts.ToList();

            foreach (var item in latestDataList)
            {
                if (item.ProductId == id)
                {
                    dbContext.Carts.Remove(item);
                    dbContext.SaveChanges();
                }
            }

            Calculate();
            return RedirectToAction("Cart");
        }

        [HttpGet]
        public IActionResult Calculate()
        {
            if (HttpContext.Session.GetString("AuthUser") != null)
            {
                MultiModel multi = new MultiModel();
                var user = HttpContext.Session.GetString("AuthUser");
                multi.cart = dbContext.Carts.Where(x => x.Username == user).ToList();

                List<Cart> totalPrice = new List<Cart>();

                var num = multi.cart.Sum(x => x.TotalPrice);

                multi.subtotal = num;

                var total = num + 40;

                HttpContext.Session.SetString("SubTotal", Convert.ToString(num));
                HttpContext.Session.SetString("Total", Convert.ToString(total));

                return RedirectToAction("Cart");
            }
            else if (HttpContext.Session.GetString("Guest") != null)
            {
                MultiModel multi = new MultiModel();
                var user = HttpContext.Session.GetString("Guest");
                multi.cart = dbContext.Carts.Where(x => x.Username == user).ToList();

                List<Cart> totalPrice = new List<Cart>();

                var num = multi.cart.Sum(x => x.TotalPrice);

                multi.subtotal = num;

                var total = num + 40;

                HttpContext.Session.SetString("SubTotal", Convert.ToString(num));
                HttpContext.Session.SetString("Total", Convert.ToString(total));

                return RedirectToAction("Cart");
            }
            else
            {
                return RedirectToAction("_NotFound", "Auth");
            }
        }

        public IActionResult Checkout()
        {
            if (HttpContext.Session.GetString("AuthUser") != null)
            {
                HttpContext.Session.Remove("Guest");
                HttpContext.Session.Remove("UpdateUserGuest");
                List<Cart> userDetails = dbContext.Carts.ToList();

                string user = HttpContext.Session.GetString("AuthUser");

                foreach (var item in userDetails)
                {
                    item.Username = user;
                }

                dbContext.Carts.UpdateRange(userDetails);
                dbContext.SaveChanges();
                HttpContext.Session.SetString("Checkout", user);
                return RedirectToAction("Payment");
            }
            else if (HttpContext.Session.GetString("Guest") != null)
            {
                HttpContext.Session.SetString("PromoteGuest", "NowUser");
                return RedirectToAction("Login", "Auth");
            }
            else
            {
                return RedirectToAction("_NotFound", "Auth");
            }
        }

        public IActionResult Payment()
        {
            if (HttpContext.Session.GetString("Checkout") != null)
            {
                var user = HttpContext.Session.GetString("Checkout");
                if (FetchAddress(user) == "Error")
                {
                    TempData["PaymentAddress"] = "Add Address To Checkout!";
                    TempData["PaymentAddress1"] = "Update Address";
                    return RedirectToAction("UserProfile");
                }
                else
                {
                    ViewBag.Address = FetchAddress(user);
                }
                MultiModel multimodel = new MultiModel();
                ViewBag.User = user;
                multimodel.cart = dbContext.Carts.Where(x => x.Username == user).ToList();
                multimodel.Products = dbContext.Products.ToList();

                ViewBag.subtotal = multimodel.cart.Sum(x => x.TotalPrice);
                ViewBag.total = ViewBag.subtotal + 40;

                return View(multimodel);
            }
            else
            {
                return RedirectToAction("_NotFound", "Auth");
            }
        }

        [HttpPost]
        public IActionResult PaySuccessCard(string cardname, string cardnum, string expiry, string cvv)
        {
            if (HttpContext.Session.GetString("Checkout") != null)
            {
                var user = HttpContext.Session.GetString("AuthUser");
                MultiModel multimodel = new MultiModel();

                multimodel.cart = dbContext.Carts.Where(x => x.Username == user).ToList();
                multimodel.Products = dbContext.Products.ToList();

                ViewBag.subtotal = multimodel.cart.Sum(x => x.TotalPrice);
                ViewBag.total = ViewBag.subtotal + 40;

                foreach (var item in multimodel.cart)
                {
                    foreach (var item2 in multimodel.Products)
                    {
                        if (item.ProductId == item2.ProductId)
                        {
                            int? basketid = item.BasketID;
                            Order orderDetails = new Order()
                            {
                                OrderNo = basketid + "/" + item.CartId + "." + DateTime.Now,
                                ProductId = item.ProductId,
                                Quantity = Convert.ToInt32(item.Quantity),
                                Status = "preparing",
                                OrderDate = DateTime.Now.Date,
                                Username = HttpContext.Session.GetString("AuthUser"),
                                Product = item2
                            };

                            dbContext.Orders.Add(orderDetails);
                            dbContext.SaveChanges();
                        }
                    }
                }

                Payment details = new Payment()
                {
                    Name = cardname,
                    CardNo = cardnum,
                    ExpiryDate = expiry,
                    CvvNo = Convert.ToInt32(cvv),
                    PaymentMode = "VisaCard",
                };

                foreach (var item in multimodel.cart)
                {
                    int? basketid = item.BasketID;
                    details.Orders = dbContext.Orders.Where(x => x.OrderNo == basketid + "/" + item.CartId + "." + DateTime.Now).ToList();
                };

                dbContext.Payments.Add(details);
                dbContext.SaveChanges();

                dbContext.Carts.RemoveRange(multimodel.cart);
                dbContext.SaveChanges();
            }
            else
            {
                return RedirectToAction("ForbiddenPage", "Auth");
            }
            HttpContext.Session.SetString("PaymentDone", "Payment-Success!!");
            HttpContext.Session.Remove("Checkout");
            HttpContext.Session.Remove("ActiveCart");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult PaySuccessUPI(string upiID)
        {
            if (HttpContext.Session.GetString("Checkout") != null)
            {
                MultiModel multimodel = new MultiModel();

                multimodel.cart = dbContext.Carts.ToList();
                multimodel.Products = dbContext.Products.ToList();

                ViewBag.subtotal = multimodel.cart.Sum(x => x.TotalPrice);
                ViewBag.total = ViewBag.subtotal + 40;

                foreach (var item in multimodel.cart)
                {
                    foreach (var item2 in multimodel.Products)
                    {
                        if (item.ProductId == item2.ProductId)
                        {
                            int? basketid = item.BasketID;
                            Order orderDetails = new Order()
                            {
                                OrderNo = basketid + "/" + item.CartId + "." + DateTime.Now,
                                ProductId = item.ProductId,
                                Quantity = Convert.ToInt32(item.Quantity),
                                Status = "preparing",
                                OrderDate = DateTime.Now.Date,
                                Username = HttpContext.Session.GetString("AuthUser"),
                                Product = item2
                            };

                            dbContext.Orders.Add(orderDetails);
                            dbContext.SaveChanges();
                        }
                    }
                }

                Payment details = new Payment()
                {
                    Name = HttpContext.Session.GetString("AuthUser"),
                    PaymentMode = "Online UPI",
                    Address = upiID
                };

                foreach (var item in multimodel.cart)
                {
                    int? basketid = item.BasketID;
                    details.Orders = dbContext.Orders.Where(x => x.OrderNo == basketid + "/" + item.CartId + "." + DateTime.Now).ToList();
                };

                dbContext.Payments.Add(details);
                dbContext.SaveChanges();

                dbContext.Carts.RemoveRange(multimodel.cart);
                dbContext.SaveChanges();
            }
            else
            {
                return RedirectToAction("ForbiddenPage", "Auth");
            }
            HttpContext.Session.SetString("PaymentDone", "Payment-Success!!");
            HttpContext.Session.Remove("Checkout");
            HttpContext.Session.Remove("ActiveCart");
            return RedirectToAction("Index");
        }

        public IActionResult LogOut()
        {
            if (HttpContext.Session.GetString("AuthUser") != null)
            {
                Response.Cookies.Delete("Authorization");
                HttpContext.Session.Remove("AuthUser");
            }

            TempData["LoggedOut"] = "Logged Out";

            return RedirectToAction("Index");
        }

        public IActionResult About()
        {
            return View();
        }

        //public IActionResult UserOrders()
        //{
        //    DualDetailModel ddm = new DualDetailModel();

        //    string user = HttpContext.Session.GetString("AuthUser");

        //    UserOrder userOrder = new UserOrder();

        //    if (user != null)
        //    {
        //        ddm.orders = dbContext.Orders.Where(x => x.Username == user).ToList();

        //        foreach(var i in ddm.orders)
        //        {
        //            string[] basketID = i.OrderNo.Split("/");

        //        }

        //    }
        //    return RedirectToAction("_NotFound", "Auth");

        private dualModel FeedMenuData(dualModel d)
        {
            dualModel dual = new dualModel();

            var data = dbContext.Products.ToList();
            var cat = dbContext.Categories.ToList();

            dual.dualProductList = data;
            dual.dualCatList = cat;
            Cart carting = new Cart();
            if (HttpContext.Session.GetString("AuthUser") != null)
            {
                carting.BasketID = Convert.ToInt32(HttpContext.Session.GetString("BasketID"));
                carting.ProductId = d.dualProduct.ProductId;
                carting.Quantity = 1;
                carting.TotalPrice = d.dualProduct.Price;
                carting.Username = HttpContext.Session.GetString("AuthUser");
            }
            else if (HttpContext.Session.GetString("Guest") != null)
            {
                carting.BasketID = Convert.ToInt32(HttpContext.Session.GetString("BasketID"));
                carting.ProductId = d.dualProduct.ProductId;
                carting.Quantity = 1;
                carting.TotalPrice = d.dualProduct.Price;
                carting.Username = HttpContext.Session.GetString("Guest");
            }

            var data3 = dbContext.Carts.FirstOrDefault(x => x.ProductId == carting.ProductId && x.Username == carting.Username);
            Calculate();

            if (data3 == null)
            {
                dbContext.Carts.Add(carting);
                dbContext.SaveChanges();
            }
            else
            {
                data3.Quantity++;
                data3.TotalPrice += d.dualProduct.Price;
                dbContext.Carts.Update(data3);
                dbContext.SaveChanges();
            }

            TempData["itemadded"] = "Item Added";

            return dual;
        }

        private string FetchAddress(string user)
        {
            FetchAddClass fetch = new FetchAddClass();

            fetch.username = user;
            //json format
            string data = JsonConvert.SerializeObject(fetch);

            StringContent content = new StringContent(data, Encoding.UTF8, "application/JSON");

            HttpResponseMessage res = client.PostAsync(urlFetchAddress, content).Result;

            if (res.IsSuccessStatusCode)
            {
                //decode data

                string recieved = res.Content.ReadAsStringAsync().Result;

                return recieved;
            }
            else
            {
                return "Error";
            }
        }
    }
}