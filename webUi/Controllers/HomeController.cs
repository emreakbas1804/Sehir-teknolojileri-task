using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json;
using webApi.Identity;
using webApi.Models;
using webUi.Extensions;
using webUi.Models;
using static webApi.Program;
using static webUi.Program;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace webUi.Controllers
{
    public class HomeController : Controller
    {

        public async Task<IActionResult> Index()
        {
            var products = new List<Products>();
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync("http://localhost:4200/home/products"))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    products = JsonConvert.DeserializeObject<List<Products>>(apiResponse);
                }
            }
            return View(products);
        }


        public async Task<IActionResult> ProductDetails(int id)
        {            
            var product = new Products();
            using (var httpClient = new HttpClient())
            {                
                using (var response = await httpClient.GetAsync($"http://localhost:4200/home/product/{id}"))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    product = JsonConvert.DeserializeObject<Products>(apiResponse);                    
                }
            }
            return View(product);
        }

    
        [HttpPost]
        public async Task<IActionResult> RegisterUser(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData.Put("message", new AlertMessage()
                {
                    Title = "Kayıt Başarısız",
                    Message = "Panele Giriş Yapabilmek İçin Mail Adresi ve Parola Girilmeli",
                    AlertType = "danger"
                });
                return View(model);
            }
            var newUser = new RegisterModel()
            {
                FullName = model.FullName,
                Email = model.Email,
                Password = model.Password
            };
            var serializeNewUser = JsonConvert.SerializeObject(newUser);
            StringContent stringContent = new StringContent(serializeNewUser, Encoding.UTF8, "application/json");

            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.PostAsync($"http://localhost:4200/home/register", stringContent))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        TempData.Put("message", new AlertMessage()
                        {
                            Title = "Kayıt Oluşturuldu",
                            Message = "Kullanıcı başarıyla oluşturuldu.",
                            AlertType = "success"
                        });
                    }
                    else
                    {
                        TempData.Put("message", new AlertMessage()
                        {
                            Title = "Hata",
                            Message = "Kullanıcı kaydı zaten veri tabanında bulunmaktadır.",
                            AlertType = "danger"
                        });
                    }
                }
            }
            return View();
        }
    
      
        [HttpGet]
        public IActionResult RegisterUser()
        {
            return View();
        }

   
        [HttpPost]
        public async Task<IActionResult> LoginUser(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData.Put("message", new AlertMessage()
                {
                    Title = "Giriş Başarısız",
                    Message = "Panele Giriş Yapabilmek İçin Mail Adresi ve Parola Girilmeli",
                    AlertType = "danger"
                });
                return View(model);
            }
            var loginUser = new LoginModel()
            {
                Email = model.Email,
                Password = model.Password
            };
            var serializeNewUser = JsonConvert.SerializeObject(loginUser);
            StringContent stringContent = new StringContent(serializeNewUser, Encoding.UTF8, "application/json");
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.PostAsync($"http://localhost:4200/home/login", stringContent))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var result = response.Content.ReadAsStringAsync();
                        HttpContext.Session.SetString("JwtValue", result.Result);
                        var roleId = await setSessionRole();
                        if(roleId =="1")
                        {
                            
                            return RedirectToAction("index", "admin");
                        }                        
                        return RedirectToAction("index", "panel");
                    }
                    else
                    {
                        TempData.Put("message", new AlertMessage()
                        {
                            Title = "Hata",
                            Message = "Mail adresi veya parola hatalı.",
                            AlertType = "danger"
                        });
                    }
                }
            }

            return View();
        }


        [HttpGet]
        public IActionResult LoginUser()
        {
            return View();
        }


        public async Task<string> setSessionRole ()
        {
            var jwtValue = HttpContext.Session.GetString("JwtValue");            
            var products = new List<Products>();
            using (var httpClient = new HttpClient())
            {                
                using (var response = await httpClient.GetAsync($"http://localhost:4200/home/getRole/{jwtValue}"))
                {
                    var result = response.Content.ReadAsStringAsync();
                    return result.Result;
                }
            }
            
        }

        


    }
}
