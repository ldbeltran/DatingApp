using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dto;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers {
    [Route ("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;
        public AuthController (IAuthRepository authRepository, IConfiguration configuration) {
            _configuration = configuration;
            _authRepository = authRepository;
        }

        [HttpPost ("register")]
        public async Task<IActionResult> Register (UserForRegisterDto userForRegisterDto) {
            userForRegisterDto.Username = userForRegisterDto.Username.ToLower ();
            if (await _authRepository.UserExists (userForRegisterDto.Username)) {
                return BadRequest ("User name already exists");
            }

            var user = new User () {
                Username = userForRegisterDto.Username
            };

            var createdUser = await _authRepository.Register (user, userForRegisterDto.Password);

            return StatusCode (201);
        }

        [HttpPost ("login")]
        public async Task<IActionResult> Login (UserForLoginDto userForLogin) {
            var user = await _authRepository.Login (userForLogin.Username, userForLogin.Password);

            if (user == null) {
                return Unauthorized ();
            }

            var claims = new [] {
                new Claim (ClaimTypes.NameIdentifier, user.Id.ToString ()),
                new Claim (ClaimTypes.Name, user.Username)
            };

            var key = new SymmetricSecurityKey (Encoding.UTF8.GetBytes (_configuration.GetSection("AppSettings:Token").Value));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new {
                token = tokenHandler.WriteToken(token)
            });

        }
    }
}