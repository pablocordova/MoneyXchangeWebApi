using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MoneyXchangeWebApi.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MoneyXchangeWebApi.Controllers
{
    [Route("api/[controller]")]
    public class LoginController : Controller
    {
        // POST api/<controller>
        [HttpPost]
        public IActionResult Post([FromBody]UserInfo userIndo)
        {
            dynamic user = new ExpandoObject();

            if (userIndo.username == "pablo" && userIndo.password == "pass")
            {
                user.token = BuildToken(userIndo);
                return Ok(user);
            }
            else
            {
                user.message = "invalid credentials";
            }

            return Ok(user);

        }

        private IActionResult BuildToken(UserInfo userIndo)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.UniqueName, userIndo.username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("secretkey132456belatrix"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddHours(72);

            JwtSecurityToken token = new JwtSecurityToken(
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = expiration
            });
        }

    }

    public class UserInfo
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    
}
