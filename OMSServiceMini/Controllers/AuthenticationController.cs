using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using OMSServiceMini.Services.Authentication;

namespace OMSServiceMini.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : Controller
    {
        private readonly UserManager<ApplicationIdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration configuration;

        public AuthenticationController(UserManager<ApplicationIdentityUser> _userManager, RoleManager<IdentityRole> _roleManager, IConfiguration _configuration)
        {
            userManager = _userManager;
            roleManager = _roleManager;
            configuration = _configuration;
        }

        #region Register all roles
        // POST api/authentication/register
        [HttpPost]
        [Route("register_admin")]
        public async Task<IActionResult> Register_Admin([FromBody] RegistrationModel registerModel)
        {
            var userExists = await userManager.FindByNameAsync(registerModel.UserName);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            var createNewUser = new ApplicationIdentityUser()
            {
                UserName = registerModel.UserName,
                SecurityStamp = Guid.NewGuid().ToString(),
                Email = registerModel.Email
            };

            // если не получится создать пользователя, ошибка 500
            // 
            // если ролей не существует - создаем
            var result = await userManager.CreateAsync(createNewUser, registerModel.Password);

            if (!result.Succeeded)// ошибка была из-за отсутствия цифр в пароле, а так же в отсутствии заглавных букв в пароле
                return StatusCode(StatusCodes.Status500InternalServerError);

            #region Понадобится для добавления новых ролей в базу, каждый раз проверять существует ли эта роль не нужно
            //if (!await roleManager.RoleExistsAsync(UserRoles.RoleAdmin)) // Получает флаг, указывающий, существует ли указанное имя роли
            //    await roleManager.CreateAsync(new IdentityRole(UserRoles.RoleAdmin)); //Создает указанную роль в постоянном хранилище
            //if (!await roleManager.RoleExistsAsync(UserRoles.RoleUser))
            //    await roleManager.CreateAsync(new IdentityRole(UserRoles.RoleUser));
            //if (!await roleManager.RoleExistsAsync(UserRoles.RoleGuest))
            //    await roleManager.CreateAsync(new IdentityRole(UserRoles.RoleGuest));
            #endregion

            if (await roleManager.RoleExistsAsync(UserRoles.RoleAdmin))
                await userManager.AddToRoleAsync(createNewUser, UserRoles.RoleAdmin); // Добавляет указанного пользователя в указанную роль

            return Ok();
        }


        // POST api/authentication/register_user
        [HttpPost]
        [Route("register_user")]
        public async Task<IActionResult> Register_User([FromBody] RegistrationModel registerModel)
        {
            var userExists = await userManager.FindByNameAsync(registerModel.UserName);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            var createNewUser = new ApplicationIdentityUser()
            {
                UserName = registerModel.UserName,
                SecurityStamp = Guid.NewGuid().ToString(),
                Email = registerModel.Email
            };

            // если не получится создать пользователя, ошибка 500
            // 
            // если ролей не существует - создаем
            var result = await userManager.CreateAsync(createNewUser, registerModel.Password);

            if (!result.Succeeded)// ошибка была из-за отсутствия цифр в пароле, а так же в отсутствии заглавных букв в пароле
                return StatusCode(StatusCodes.Status500InternalServerError);

            #region Понадобится для добавления новых ролей в базу, каждый раз проверять существует ли эта роль не нужно
            //if (!await roleManager.RoleExistsAsync(UserRoles.RoleAdmin)) // Получает флаг, указывающий, существует ли указанное имя роли
            //    await roleManager.CreateAsync(new IdentityRole(UserRoles.RoleAdmin)); //Создает указанную роль в постоянном хранилище
            //if (!await roleManager.RoleExistsAsync(UserRoles.RoleUser)) // Получает флаг, указывающий, существует ли указанное имя роли
            //    await roleManager.CreateAsync(new IdentityRole(UserRoles.RoleUser)); //Создает указанную роль в постоянном хранилище

            //if (!await roleManager.RoleExistsAsync(UserRoles.RoleGuest)) // Получает флаг, указывающий, существует ли указанное имя роли
            //    await roleManager.CreateAsync(new IdentityRole(UserRoles.RoleGuest)); //Создает указанную роль в постоянном хранилище
            #endregion

            if (await roleManager.RoleExistsAsync(UserRoles.RoleUser))
                await userManager.AddToRoleAsync(createNewUser, UserRoles.RoleUser); // Добавляет указанного пользователя в указанную роль

            return Ok();
        }
        #endregion


        #region Login
        //POST api/authentication/login
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            var user = await userManager.FindByNameAsync(loginModel.UserName);
            if (user != null && await userManager.CheckPasswordAsync(user, loginModel.Password))
            {
                var userRoles = await userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));

                var token = new JwtSecurityToken(
                    issuer: configuration["JWT:ValidIssuer"],
                    audience: configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(10),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token)
                });
            }
            return Unauthorized();
        }
        #endregion

    }
}