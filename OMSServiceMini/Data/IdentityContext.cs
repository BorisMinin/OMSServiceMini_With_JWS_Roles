using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OMSServiceMini.Services.Authentication;

namespace OMSServiceMini.Data
{
    //migration:
    //Add-Migration InitialCreate -Context IdentityContext -OutputDir Migrations
    //Update-Database -Context IdentityContext
    public class IdentityContext : IdentityDbContext<ApplicationIdentityUser>
    {
        public IdentityContext(DbContextOptions<IdentityContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source=Data/IdentityDB.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}