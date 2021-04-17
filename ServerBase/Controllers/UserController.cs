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
        private readonly ServerDbContext _gameDbContext;
        private readonly ILogger<UserController> _logger;

        private readonly JwtTokenConfig _tokenConfig;

        public UserController(ILogger<UserController> logger, ServerDbContext dbContext, JwtTokenConfig jwtTokenConfig)
        {
            _logger = logger;
            _accountDbContext = dbContext;
            _gameDbContext = dbContext;
            _tokenConfig = jwtTokenConfig;
        }

        [AllowAnonymous]
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] UserSignUpRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await _accountDbContext.User.FirstOrDefaultAsync(i => i.Name == request.Name);
            if (user != null)
            {
                return BadRequest();
            }

            user = new Entity.User()
            {
                Name = request.Name,
                Password = request.Password,
                CreateTime = DateTime.UtcNow,
                Deleted = false,
            };
            await _accountDbContext.User.AddAsync(user);
            await _accountDbContext.SaveChangesAsync();

            // 기본 설정
            var gold = new Entity.Point()
            {
                UserId = user.Id,
                Type = Entity.PointType.Gold,
                Quantity = 100,
                CreateTime = DateTime.UtcNow,
                Deleted = false,
            };
            await _gameDbContext.Point.AddAsync(gold);

            var silver = new Entity.Point()
            {
                UserId = user.Id,
                Type = Entity.PointType.Silver,
                Quantity = 100,
                CreateTime = DateTime.UtcNow,
                Deleted = false,
            };
            await _gameDbContext.Point.AddAsync(silver);
            await _gameDbContext.SaveChangesAsync();

            return Ok(new
            {
                user = new
                {
                    id = user.Id,
                    name = user.Name,
                    createTime = user.CreateTime,
                    role = "Admin",
                },
                points = new[]
                {
                    new
                    {
                        type = gold.Type.ToString(),
                        quantity = gold.Quantity,
				    },
                    new
                    {
                        type = silver.Type.ToString(),
                        quantity = silver.Quantity,
				    },
			    },
            });
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
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, "Admin"),
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

            return Ok(new
            {
                name = request.Name,
                accessToken = accessToken,
                role = "Admin",
            });
        }
    }
}