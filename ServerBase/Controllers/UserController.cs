using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ServerBase.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ServerDbContext _accountDbContext;
        private readonly ILogger<UserController> _logger;

        private readonly JwtTokenConfig _tokenConfig;

        public UserController(ILogger<UserController> logger, ServerDbContext dbContext, JwtTokenConfig jwtTokenConfig)
        {
            _logger = logger;
            _accountDbContext = dbContext;
            _tokenConfig = jwtTokenConfig;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await _accountDbContext.User.FirstOrDefaultAsync(i => i.Name == request.Name && i.Password == request.Password);
            if (user == null)
            {
                return Unauthorized();
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, request.Name),
            };

            var now = DateTime.UtcNow;
            var shouldAddAudienceClaim = string.IsNullOrWhiteSpace(claims?.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Aud)?.Value);
            var jwtToken = new JwtSecurityToken(
                _tokenConfig.Issuer,
                shouldAddAudienceClaim ? _tokenConfig.Audience : string.Empty,
                claims,
                expires: now.AddMinutes(_tokenConfig.AccessTokenExpiration),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_tokenConfig.Secret)), SecurityAlgorithms.HmacSha256Signature));
            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            _logger.LogInformation($"Login Successful: {request.Name},Admin,{accessToken}");

            return Ok(new UserLoginResponse()
            {
                Name = request.Name,
                AccessToken = accessToken,
                Role = "Admin",
            });
        }
    }
}