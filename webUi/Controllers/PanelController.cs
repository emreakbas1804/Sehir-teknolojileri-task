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
using webApi.Models;
using webUi.Extensions;
using webUi.Models;

namespace webUi.Controllers
{
    public class PanelController : Controller
    {

        public IActionResult logOut()
        {
            HttpContext.Session.Remove("JwtValue");
            return RedirectToAction("index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var jwtValue = HttpContext.Session.GetString("JwtValue");
            if (jwtValue == null)
            {
                return RedirectToAction("loginuser", "home");
            }
            // else
            // {
                                
            //     using (var httpClient = new HttpClient())
            //     {
            //         using (var response = await httpClient.GetAsync($"http://localhost:4200/home/getRole/{jwtValue}"))
            //         {
            //             var result = response.Content.ReadAsStringAsync();
            //             var Role= result.Result;
            //             if(Role =="1")
            //             {
            //                 return RedirectToAction("index", "admin");
            //             }
            //         }
            //     }
            // }

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

        [HttpPost]
        public async Task<IActionResult> Index(string search)
        {
            var products = new List<Products>();
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync($"http://localhost:4200/home/products/{search}"))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    products = JsonConvert.DeserializeObject<List<Products>>(apiResponse);
                }
            }
            return View(products);
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            var jwtValue = HttpContext.Session.GetString("JwtValue");
            if (jwtValue == null)
            {
                return RedirectToAction("loginuser", "home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(Products model, IFormFile file)
        {
            var jwtValue = HttpContext.Session.GetString("JwtValue");
            if (!ModelState.IsValid)
            {
                TempData.Put("message", new AlertMessage()
                {
                    Title = "Kayıt Başarısız",
                    Message = "Ürün kaydı için bütün bilgiler girilmeli.",
                    AlertType = "danger"
                });
                return View(model);
            }

            var newProduct = new Products()
            {
                ProductName = model.ProductName,
                ProductPrice = model.ProductPrice,
                ProductImageUrl = model.ProductImageUrl,
                ProductDesciription = model.ProductDesciription
            };
            if (file != null)
            {
                var extention = Path.GetExtension(file.FileName);
                var RandomName = string.Format($"{Guid.NewGuid()}{extention}");
                newProduct.ProductImageUrl = RandomName;
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products", RandomName);
                using (var stream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
                {
                    await file.CopyToAsync(stream);
                    stream.Flush();
                }
            }
            var serializenewProduct = JsonConvert.SerializeObject(newProduct);
            StringContent stringContent = new StringContent(serializenewProduct, Encoding.UTF8, "application/json");
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtValue);
                using (var response = await httpClient.PostAsync($"http://localhost:4200/home/addProduct", stringContent))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        TempData.Put("message", new AlertMessage()
                        {
                            Title = "Ürün eklendi",
                            Message = "Ürün veri tabanına başarıyla oluşturuldu.",
                            AlertType = "success"
                        });
                        return View();
                    }
                    if (response.StatusCode.ToString() == "Unauthorized")
                    {
                        return RedirectToAction("loginuser", "home");
                    }
                }
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
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
        public async Task<IActionResult> EditProduct(Products model, IFormFile file)
        {
            var jwtValue = HttpContext.Session.GetString("JwtValue");
            var newProduct = new Products()
            {
                Id = model.Id,
                ProductName = model.ProductName,
                ProductPrice = model.ProductPrice,
                ProductImageUrl = model.ProductImageUrl,
                ProductDesciription = model.ProductDesciription
            };
            if (file != null)
            {
                var extention = Path.GetExtension(file.FileName);
                var RandomName = string.Format($"{Guid.NewGuid()}{extention}");
                newProduct.ProductImageUrl = RandomName;
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products", RandomName);
                using (var stream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
                {
                    await file.CopyToAsync(stream);
                    stream.Flush();
                }
            }
            var serializenewProduct = JsonConvert.SerializeObject(newProduct);
            StringContent stringContent = new StringContent(serializenewProduct, Encoding.UTF8, "application/json");
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtValue);
                using (var response = await httpClient.PostAsync($"http://localhost:4200/home/editProduct", stringContent))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        TempData.Put("message", new AlertMessage()
                        {
                            Title = "Ürün güncellemesi başarılı",
                            Message = "Ürün veri tabanına başarıyla güncellendi.",
                            AlertType = "success"
                        });
                    }
                    if (response.StatusCode.ToString() == "Unauthorized")
                    {
                        TempData.Put("message", new AlertMessage()
                        {
                            Title = "Hata",
                            Message = "Yetkisiz alan",
                            AlertType = "danger"
                        });
                        return RedirectToAction("loginuser", "home");
                    }
                }
            }

            return View();
        }

        public async Task<IActionResult> RemoveProduct(int id)
        {
            var jwtValue = HttpContext.Session.GetString("JwtValue");
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtValue);
                using (var response = await httpClient.GetAsync($"http://localhost:4200/home/removeProduct/{id}"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        TempData.Put("message", new AlertMessage()
                        {
                            Title = "Ürün Silindi",
                            Message = id + " Numaralı ürün veri tabanına başarıyla kaldırıldı.",
                            AlertType = "success"
                        });
                    }
                    if (response.StatusCode.ToString() == "Unauthorized")
                    {
                        return RedirectToAction("loginuser", "home");
                    }
                }
            }
            return RedirectToAction("index", "panel");
        }





    }
}