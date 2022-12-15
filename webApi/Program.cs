using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using webApi.Identity;
using webApi.Models;
using webUi.Extensions;

namespace webApi
{
    public class Program
    {
        public class SehirTeknolojileriContext : IdentityDbContext<User>
        {


            public SehirTeknolojileriContext(DbContextOptions<SehirTeknolojileriContext> options)
                : base(options)
            {

            }
            public DbSet<UserTokens> UserTokens { get; set; }             
            public DbSet<Products> Products { get; set; }
        }
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().MigrateDatabase().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
