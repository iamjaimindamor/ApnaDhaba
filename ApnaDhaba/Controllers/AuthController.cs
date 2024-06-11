using ApnaDhaba.Api_Models;
using ApnaDhaba.Models.Other;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace ApnaDhaba.Controllers
{
    public class AuthController : Controller
    {
        private string urlLogin = "https://localhost:7215/api/Auth/Login";
        private string urlRegister = "https://localhost:7215/api/Auth/Register";
        private string urlCon = "https://localhost:7215/api/Auth/Confirm";
        private string urlReset = "https://localhost:7215/api/Auth/Reset";

        private HttpClient client = new HttpClient();

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(UserModel user, string confirmPassword)
        {
            if (ModelState.IsValid)
            {
                if (user.Password != confirmPassword)
                {
                    TempData["confirmPasswordFailed"] = "Password Do Not Match";
                    return View();
                }
                else
                {
                    //serialize the data
                    string data = JsonConvert.SerializeObject(user);
                    //Encode in JSON format
                    StringContent content = new StringContent(data, Encoding.UTF8, "application/JSON");
                    //send the request and store the response

                    try
                    {
                        HttpResponseMessage response = client.PostAsync(urlRegister, content).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            TempData["RegisterSuccess"] = "User Successfully Registered";

                            return View();
                        }
                        else
                        {
                            return RedirectToAction("_NotFound");
                        }
                    }
                    catch
                    {
                        TempData["APIDown"] = "API Server is Down";
                        return RedirectToAction("SessionEnded");
                    }
                }
            }
            else return View(user);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest login)
        {
            CookieOptions cookie = new CookieOptions();
            //cookie.HttpOnly = true;

            if (login.Username != null)
            {
                //serailizing the content
                var data = JsonConvert.SerializeObject(login);
                //content
                StringContent content = new StringContent(data, Encoding.UTF8, "application/JSON");
                //send the response
                try
                {
                    HttpResponseMessage response = client.PostAsync(urlLogin, content).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string recieved = await response.Content.ReadAsStringAsync();
                       
                        HttpContext.Response.Cookies.Append("Authorization", recieved);

                        if (recieved != null)
                        {
                            HttpContext.Session.SetString("JWT", recieved);
                            HttpContext.Session.SetString("AuthUser", login.Username);
                            if (HttpContext.Session.GetString("Guest") != null)
                            {
                                if (HttpContext.Session.GetString("PromoteGuest") != null)
                                {
                                    HttpContext.Session.SetString("UpdateUserGuest", login.Username);

                                    HttpContext.Session.Remove("PromoteGuest");
                                }
                            }
                        }
                        else
                        {
                            TempData["Failed"] = "Access Token Is Not Recieved...";
                            return View();
                        }

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        TempData["Failed"] = "Invalid Credentials";
                        return View();
                    }
                }
                catch
                {
                    TempData["APIDown"] = "API Server is Down";
                    return RedirectToAction("SessionEnded");
                }
            }
            else return View(login);
        }

        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgetPassword(UserModel user)
        {
            if (user != null && ModelState.IsValid)
            {
                string data = JsonConvert.SerializeObject(user);

                StringContent content = new StringContent(data, Encoding.UTF8, "application/JSON");
                //send the request and store the response
                try
                {
                    HttpResponseMessage response = client.PostAsync(urlCon, content).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        HttpContext.Session.SetString("ResetSession", user.Username);
                        TempData["userConfirmed"] = "UserFound";
                        return View();
                    }
                    else
                    {
                        TempData["NotFoundForgot"] = "User Not Found";
                        return View();
                    }
                }
                catch
                {
                    TempData["APIDown"] = "API Server is Down";
                    return RedirectToAction("SessionEnded");
                }
            }
            else
            {
                TempData["NotFoundForgot"] = "Username is required";
                return View(user);
            }
        }

        public IActionResult Reset()
        {
            try
            {
                if (HttpContext.Session.GetString("AuthUser") != null || HttpContext.Session.GetString("ResetSession") != null)
                {
                    return View();
                }
                else
                {
                    throw new Exception("Error Session Not Created");
                }
            }
            catch
            {
                TempData["SessionNotCreated"] = "Session Not Created";
                return RedirectToAction("SessionEnded");
            }
        }

        [HttpPost]
        public IActionResult Reset(Reset newPass, string confirmPassword)
        {
            string resetToken = "null";

            if (newPass.Password != confirmPassword)
            {
                TempData["confirmPasswordReset"] = "Password Do Not Match";
                return View();
            }
            if (HttpContext.Session.GetString("ResetSession") != null || HttpContext.Session.GetString("AuthUser") != null)
            {
                if (HttpContext.Session.GetString("ResetSession") != null)
                {
                    newPass.username = HttpContext.Session.GetString("ResetSession");
                }
                else if (HttpContext.Session.GetString("AuthUser") != null)
                {
                    resetToken = Request.Cookies["Authorization"];
                    newPass.username = HttpContext.Session.GetString("AuthUser");
                }

                if (HttpContext.Session.GetString("AuthUser") != null)
                {
                    string data = JsonConvert.SerializeObject(newPass);

                    StringContent content = new StringContent(data, Encoding.UTF8, "application/JSON");
                    var request = new HttpRequestMessage(HttpMethod.Post, urlReset);
                    request.Content = content;
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", resetToken);

                    HttpResponseMessage response = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result;

                    try
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            HttpContext.Session.Remove("ResetSession");
                            TempData["ResetSuccess"] = "Password Resetted Successfully";
                            return RedirectToAction("Login");
                        }
                        else
                        {
                            HttpContext.Session.Remove("ResetSession");
                            TempData["SuccessReset"] = "Password Reset Failed";
                            return View();
                        }
                    }
                    catch
                    {
                        TempData["APIDown"] = "API Server is Down";
                        return RedirectToAction("SessionEnded");
                    }
                }
                else if(HttpContext.Session.GetString("ResetSession") != null)
                {
                    string data = JsonConvert.SerializeObject(newPass);

                    StringContent content = new StringContent(data, Encoding.UTF8, "application/JSON");

                    try
                    {
                        HttpResponseMessage response = client.PostAsync(urlReset, content).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            HttpContext.Session.Remove("ResetSession");
                            TempData["ResetSuccess"] = "Password Resetted Successfully";
                            return RedirectToAction("Login");
                        }
                        else
                        {
                            HttpContext.Session.Remove("ResetSession");
                            TempData["SuccessReset"] = "Password Reset Failed";
                            return View();
                        }
                    }
                    catch
                    {
                        TempData["APIDown"] = "API Server is Down";
                        return RedirectToAction("SessionEnded");
                    }
                        
                }else return RedirectToAction("SessionEnded");
            }
            else
            {
                TempData["SessionEnded"] = "Session Ended ! !";
                return RedirectToAction("SessionEnded");
            }
        }

        public IActionResult ForbiddenPage()
        {
            return View();
        }

        public IActionResult _NotFound()
        {
            return View();
        }

        public IActionResult SessionEnded()
        {
            return View();
        }
    }
}