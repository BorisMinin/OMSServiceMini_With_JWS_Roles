using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OMSServiceMini.Data;
using OMSServiceMini.Services.Authentication;

namespace OMSServiceMini
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        //public void ConfigureServices(IServiceCollection services)
        //{
        //    //----Secret key generation-----
        //    string signingSecurityKey = Configuration.GetSection("JwtConfig").GetSection("ServiceApiKey").Value;
        //    var signingKey = new SigningSymmetricKey(signingSecurityKey);
        //    services.AddSingleton<IJwtSigningEncodingKey>(signingKey);
        //    //------------------------------

        //    // �������� ��e������� NORTHWNDContext - �������� ���� ������ �� SQL Servere
        //    string connection = Configuration.GetConnectionString("OMSDatabase");

        //    services.AddDbContext<NorthwindContext>(options =>
        //    options.UseSqlServer(connection));
        //    //-------------------------------

        //    services.AddControllers();

        //    //----JWTBearer----

        //    const string jwtSchemeName = "JwtBearer";
        //    var signingDecodingKey = (IJwtSigningDecodingKey)signingKey;
        //    services
        //        .AddAuthentication(options => 
        //        {
        //            options.DefaultAuthenticateScheme = jwtSchemeName;
        //            options.DefaultChallengeScheme = jwtSchemeName;
        //        })
        //        .AddJwtBearer(jwtSchemeName, jwtBearerOptions => 
        //        {
        //            jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
        //            {
        //                ValidateIssuerSigningKey = true,
        //                IssuerSigningKey = signingDecodingKey.GetKey(),

        //                ValidateIssuer = true,
        //                ValidIssuer = "OMSWebMini",

        //                ValidateAudience = true,
        //                ValidAudience = "OMSWebMiniClient",

        //                ValidateLifetime = true,

        //                ClockSkew = TimeSpan.FromSeconds(5)
        //            };
        //        });
        //    //----------------

        //    //string connection = Configuration.GetConnectionString("OMSDatabase");
        //    //services.AddDbContext<NorthwindContext>(options => options.UseSqlServer(connection));

        //    services.AddSwaggerDocument(config =>
        //    {
        //        config.PostProcess = document =>
        //        {
        //            document.Info.Version = "v1";
        //            document.Info.Title = "OMSServiceMini";
        //            document.Info.Description = "A simple study project ASP.NET Core web API";
        //            document.Info.Contact = new NSwag.OpenApiContact
        //            {
        //                Name = "Boris Minin",
        //                Email = "boris.minin@outlook.com",
        //                Url = "https://www.facebook.com/borisminindeveloper"
        //            };
        //            document.Info.License = new NSwag.OpenApiLicense
        //            {
        //                Name = "Look at my GitHub",
        //                Url = "https://github.com/BorisMinin"
        //            };
        //        };
        //    });

        //    // https://stackoverflow.com/questions/59199593/net-core-3-0-possible-object-cycle-was-detected-which-is-not-supported
        //    services.AddControllers().AddNewtonsoftJson(options =>
        //        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
        //}
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            //EF
            string sqlConnection = Configuration.GetConnectionString("SQLDatabase");
            services.AddDbContext<NorthwindContext>(o => o.UseSqlServer(sqlConnection));

            string sqliteConection = Configuration.GetConnectionString("SQLiteDataBase");
            services.AddDbContext<IdentityContext>(o => o.UseSqlite(sqliteConection));
            //Identity
            //������ �������� ������ �������
            services.AddIdentity<ApplicationIdentityUser, IdentityRole>(o =>
            {                
                o.Password.RequireDigit = false; // �� �������� ���� � ������� ���� � ������
                o.Password.RequiredLength = 1; // ����������� ����� ������ 
                o.Password.RequireNonAlphanumeric = false; // ����������-�������� �������
                o.Password.RequireUppercase = false; // ������� ������� (��������� �����)
                o.Password.RequireLowercase = false; // ������ ������� (��������� �����)
            }).AddEntityFrameworkStores<IdentityContext>().AddDefaultTokenProviders();
            //����� �������

            // Adding Authentication  
            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })

            // Adding Jwt Bearer  
            .AddJwtBearer(o =>
            {
                o.SaveToken = true;
                o.RequireHttpsMetadata = false;
                o.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = Configuration["JWT:ValidAudience"],
                    ValidIssuer = Configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]))
                };
            });

            // Register the Swagger services
            services.AddSwaggerDocument();

            // https://stackoverflow.com/questions/59199593/net-core-3-0-possible-object-cycle-was-detected-which-is-not-supported
            services.AddControllers().AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication(); 
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Register the Swagger generator and the Swagger UI middlewares
            app.UseOpenApi();
            app.UseSwaggerUi3();
        }
    }
}