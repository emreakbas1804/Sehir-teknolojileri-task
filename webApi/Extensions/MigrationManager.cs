using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static webApi.Program;

namespace webUi.Extensions
{
    public static class MigrationManager
    {
        public static IHost MigrateDatabase(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                using (var SehirTeknolojileriContext = scope.ServiceProvider.GetRequiredService<SehirTeknolojileriContext>())
                {
                    try
                    {
                        SehirTeknolojileriContext.Database.Migrate();
                    }
                    catch (System.Exception)
                    {

                        throw;
                    }
                }
            }
            return host;
        }
    }
}