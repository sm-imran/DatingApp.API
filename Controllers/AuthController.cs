using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        public AuthController(IAuthRepository repo, IConfiguration config)
        {
            _config = config;
            _repo = repo;

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]UserForRegisterDto UserForRegisterDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var isExists = await _repo.UserExists(UserForRegisterDto.username.ToLower());
            if (isExists)
                return BadRequest("User Name already Exists");

            var UserToCreate = new User
            {
                Username = UserForRegisterDto.username.ToLower()

            };

            var CreatedUser = await _repo.Register(UserToCreate, UserForRegisterDto.password);
            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody]UserForLoginDto UserForLoginDto)
        {
            var userFromRepo = await _repo.Login(UserForLoginDto.username.ToLower(), UserForLoginDto.password);
            if (userFromRepo == null)
                return Unauthorized();
            //generate token;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config.GetSection("AppSetting:token").Value);
            List<Claim> claims = new List<Claim>(){
                new Claim(ClaimTypes.NameIdentifier,userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name,userFromRepo.Username)
            };



            var securityDescriptor = new SecurityTokenDescriptor()
            {

                Subject = new ClaimsIdentity(claims),
                SigningCredentials =
                  new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512),
                Expires = DateTime.Now.AddDays(1)

            };
            var token = tokenHandler.CreateToken(securityDescriptor);
            var tokenstring = tokenHandler.WriteToken(token);
            return Ok(new { tokenstring });

        }

    }
}