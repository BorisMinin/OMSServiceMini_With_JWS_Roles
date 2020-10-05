using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OMSServiceMini.Services.Authenticatinon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace OMSServiceMini.Services.Data
{
    public class IdentityContext : IdentityDbContext<ApplicationUser>
    {
        public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Sourse=Data/IdentityDB.db");
        }
        //migration:
        //Add-Migration IdentityContextMininBorisMigrationTry -Context IdentityContext -OutputDir Migrations\SqliteMigrations
        //Add-Migration InitialCreate -Context MySqliteDbContext -OutputDir Migrations\SqliteMigrations
    }
}