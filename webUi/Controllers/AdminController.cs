using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using webApi.Identity;
using webApi.Models;
using webUi.Extensions;
using webUi.Models;

namespace webUi.Controllers
{
    public class AdminController : Controller
    {

        public async Task<IActionResult> Index()
        {
            var users = new List<User>();
            var jwtValue = HttpContext.Session.GetString("JwtValue");
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtValue);
                using (var response = await httpClient.GetAsync("http://localhost:4200/home/allUsers"))
                {
                    if (response.StatusCode.ToString() == "Unauthorized" || response.StatusCode.ToString() == "Forbidden")
                    {
                        TempData.Put("message", new AlertMessage()
                        {
                            Title = "Yetkisiz alan",
                            Message = "Erişim izniniz olmayan istek",
                            AlertType = "danger"
                        });
                        return RedirectToAction("loginuser", "home");
                    }
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    users = JsonConvert.DeserializeObject<List<User>>(apiResponse);
                }
            }
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateProfile(string id)
        {
            var user = new UpdateProfile();
            var jwtValue = HttpContext.Session.GetString("JwtValue");
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtValue);
                using (var response = await httpClient.GetAsync($"http://localhost:4200/home/editUser/{id}"))
                {
                    if (response.StatusCode.ToString() == "Unauthorized" || response.StatusCode.ToString() == "Forbidden")
                    {
                        TempData.Put("message", new AlertMessage()
                        {
                            Title = "Yetkisiz alan",
                            Message = "Erişim izniniz olmayan istek",
                            AlertType = "danger"
                        });
                        return RedirectToAction("loginuser", "home");
                    }
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    user = JsonConvert.DeserializeObject<UpdateProfile>(apiResponse);
                }
            }
            return View(user);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateProfile(UpdateProfile model)
        {
            var jwtValue = HttpContext.Session.GetString("JwtValue");
            var userUpdate = new UpdateProfile()
            {
                Id = model.Id,
                Email = model.Email,
                FullName = model.FullName,
                Password = model.Password
            };
            var serializeUser = JsonConvert.SerializeObject(userUpdate);
            StringContent stringContent = new StringContent(serializeUser, Encoding.UTF8, "application/json");
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtValue);
                using (var response = await httpClient.PostAsync($"http://localhost:4200/home/editUser/{model.Id}", stringContent))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        TempData.Put("message", new AlertMessage()
                        {
                            Title = "Kullanıcı güncellemesi başarılı",
                            Message = "Kullanıcı veri tabanına başarıyla güncellendi.",
                            AlertType = "success"
                        });
                    }
                    if (response.StatusCode.ToString() == "Unauthorized" || response.StatusCode.ToString() == "Forbidden")
                    {
                        TempData.Put("message", new AlertMessage()
                        {
                            Title = "Yetkisiz alan",
                            Message = "Erişim izniniz olmayan istek",
                            AlertType = "danger"
                        });
                        return RedirectToAction("loginuser", "home");
                    }
                }
            }
            return View();
        }


        public async Task<IActionResult> DeleteUser(string id)
        {
            var jwtValue = HttpContext.Session.GetString("JwtValue");
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtValue);
                using (var response = await httpClient.GetAsync($"http://localhost:4200/home/removeUser/{id}"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        TempData.Put("message", new AlertMessage()
                        {
                            Title = "Kullanıcı Silindi",
                            Message = id + " Numaralı kullanıcı veri tabanına başarıyla kaldırıldı.",
                            AlertType = "success"
                        });
                    }
                    if (response.StatusCode.ToString() == "Unauthorized" || response.StatusCode.ToString() == "Forbidden")
                    {
                        TempData.Put("message", new AlertMessage()
                        {
                            Title = "Yetkisiz alan",
                            Message = "Erişim izninin yok.",
                            AlertType = "danger"
                        });
                        return RedirectToAction("loginuser", "home");
                    }
                }
            }
            return RedirectToAction("index", "admin");            
        }


        [HttpGet]
        public IActionResult AddUser()
        {            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(RegisterModel model, IFormFile file)
        {
            var jwtValue = HttpContext.Session.GetString("JwtValue");
            if (!ModelState.IsValid)
            {
                TempData.Put("message", new AlertMessage()
                {
                    Title = "Kayıt Başarısız",
                    Message = "Kullanıcı kaydı için bütün bilgiler girilmeli.",
                    AlertType = "danger"
                });
                return View(model);
            }

            var newUser = new RegisterModel()
            {
                FullName = model.FullName,
                Email = model.Email,
                Password = model.Password,
                Role = model.Role           
            };
            
            var serializenewProduct = JsonConvert.SerializeObject(newUser);
            StringContent stringContent = new StringContent(serializenewProduct, Encoding.UTF8, "application/json");
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtValue);
                using (var response = await httpClient.PostAsync($"http://localhost:4200/home/addUser", stringContent))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        TempData.Put("message", new AlertMessage()
                        {
                            Title = "Kullanıcı eklendi",
                            Message = "Kullanıcı veri tabanına başarıyla oluşturuldu.",
                            AlertType = "success"
                        });
                    }
                    if (response.StatusCode.ToString() == "Unauthorized" || response.StatusCode.ToString() == "Forbidden")
                    {
                        TempData.Put("message", new AlertMessage()
                        {
                            Title = "Yetkisiz alan",
                            Message = "Erişim izninin yok.",
                            AlertType = "danger"
                        });
                        return RedirectToAction("loginuser", "home");
                    }
                }
            }
            return View();
        }



    }
}