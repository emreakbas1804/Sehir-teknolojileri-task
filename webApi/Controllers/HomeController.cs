using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using webApi.Identity;
using webApi.Models;
using static webApi.Program;

namespace webApi.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class HomeController : ControllerBase
    {
        private readonly SehirTeknolojileriContext context;
        private readonly IConfiguration configuration;
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;


        public HomeController(SehirTeknolojileriContext Context, IConfiguration Configuration, SignInManager<User> SignInManager, UserManager<User> UserManager, RoleManager<IdentityRole> RoleManager)
        {
            context = Context;
            configuration = Configuration;
            signInManager = SignInManager;
            userManager = UserManager;
            roleManager = RoleManager;
        }

        [HttpGet]
        [Route("products")]
        public async Task<IActionResult> Products()
        {
            var products = await context.Products.ToListAsync();
            return Ok(products);
        }


        [HttpGet]
        [Route("product/{id}")]
        public async Task<IActionResult> ProductDetails(int id)
        {
            var products = await context.Products.Where(i => i.Id == id).FirstOrDefaultAsync();
            if (products != null)
            {
                return Ok(products);
            }
            return BadRequest();
        }


        [HttpGet]
        [Route("products/{search}")]
        public async Task<IActionResult> SearchProducts(string search)
        {
            var products = await context.Products.ToListAsync();
            var filterProducts = products.Where(i => i.ProductName.ToLower().Contains(search.ToLower())).ToList();
            return Ok(filterProducts);
        }



        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> login([FromBody] LoginModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
                if (result.Succeeded)
                {
                    var userRole = await userManager.GetRolesAsync(user);
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes(configuration["Secret"]);
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                        new Claim(ClaimTypes.Name, model.Email),
                        new Claim(ClaimTypes.Role,userRole[0])
                        }),
                        Expires = DateTime.UtcNow.AddYears(1),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                    };
                    var token = tokenHandler.CreateToken(tokenDescriptor);

                    if (context.UserTokens.Where(i => i.UserId == user.Id).FirstOrDefault() == null)
                    {
                        var UserToken = new UserTokens()
                        {
                            UserId = user.Id,
                            LoginProvider = "SystemApi",
                            Name = user.Email,
                            Value = tokenHandler.WriteToken(token),
                        };
                        context.UserTokens.Add(UserToken);
                        context.SaveChanges();
                    }
                    else
                    {
                        context.UserTokens.Where(i => i.UserId == user.Id).First().Value = tokenHandler.WriteToken(token);
                        context.Update(context.UserTokens.Where(i => i.UserId == user.Id).First());
                        context.SaveChanges();
                    }
                    return Ok(tokenHandler.WriteToken(token));
                }
                else
                    return BadRequest("Giriş hatalı");
            }
            else
                return BadRequest();
        }


        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> register([FromBody] RegisterModel model)
        {
            User person = await userManager.FindByEmailAsync(model.Email);
            if (person != null)
            {
                return BadRequest();
            }
            User user = new User();
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;
            var result = await userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {                
                var RegisteredUser = context.Users.Where(i => i.Email == model.Email).FirstOrDefault();
                var role = new IdentityUserRole<string>()
                {
                    UserId = RegisteredUser.Id,
                    RoleId = "2"
                };
                context.UserRoles.Add(role);

                context.SaveChanges();
                return Ok();
            }
            else
                return BadRequest();
        }


        [HttpPost]
        [Route("addProduct")]
        [Authorize]
        public IActionResult AddProduct([FromBody] Products model)
        {
            var product = new Products()
            {
                ProductName = model.ProductName,
                ProductPrice = model.ProductPrice,
                ProductImageUrl = model.ProductImageUrl,
                ProductDesciription = model.ProductDesciription
            };
            context.Products.AddAsync(product);
            context.SaveChangesAsync();
            return Ok("ürün eklendi");
        }


        [HttpPost]
        [Route("editProduct")]
        [Authorize]
        public async Task<IActionResult> EditProduct([FromBody] Products model)
        {
            var entity = await context.Products.FindAsync(model.Id);
            entity.ProductName = model.ProductName;
            entity.ProductPrice = model.ProductPrice;
            entity.ProductDesciription = model.ProductDesciription;
            entity.ProductImageUrl = model.ProductImageUrl;
            context.Products.Update(entity);
            context.SaveChanges();
            return Ok();
        }


        [HttpGet]
        [Route("removeProduct/{id}")]
        [Authorize]
        public async Task<IActionResult> RemoveProduct(int id)
        {
            var entity = await context.Products.FindAsync(id);
            context.Products.Remove(entity);
            context.SaveChanges();
            return Ok();
        }

        [HttpGet]
        [Route("getRole/{jwt}")]
        public IActionResult UserGetRole(string jwt)
        {
            var userId = context.UserTokens.Where(i => i.Value == jwt).FirstOrDefault().UserId;
            var UserRole = context.UserRoles.Where(i => i.UserId == userId).FirstOrDefault();
            return Ok(UserRole.RoleId);
        }

        [HttpGet]
        [Route("allUsers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> getAllUsers(string jwt)
        {
            var users = await context.Users.ToListAsync();
            return Ok(users);
        }


        [HttpGet]
        [Route("editUser/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await context.Users.FindAsync(id);
            return Ok(user);
        }


        [HttpPost]
        [Route("editUser/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUser([FromBody] UpdateProfile model)
        {
            var user = await context.Users.FindAsync(model.Id);
            if (user != null)
            {
                user.FullName = model.FullName;
                user.Email = model.Email;
                if (model.Password != null)
                {
                    string token = await userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await userManager.ResetPasswordAsync(user, token, model.Password);
                }
                context.SaveChanges();
                return Ok();
            }

            return BadRequest();
        }

        [HttpGet]
        [Route("removeUser/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveUser(string id)
        {
            var entity = await context.Users.FindAsync(id);
            context.Users.Remove(entity);
            context.SaveChanges();
            return Ok();
        }

        [HttpPost]
        [Route("addUser")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddUser([FromBody] RegisterModel model)
        {
            User person = await userManager.FindByEmailAsync(model.Email);
            if (person != null)
            {
                return BadRequest();
            }
            User user = new User();
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;
            var result = await userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                var RegisteredUser = context.Users.Where(i => i.Email == model.Email).FirstOrDefault();
                var role = new IdentityUserRole<string>()
                {
                    UserId = RegisteredUser.Id,
                    RoleId = model.Role
                };
                context.UserRoles.Add(role);

                context.SaveChanges();
                return Ok();
            }
            else
                return BadRequest();
        }



    }
}