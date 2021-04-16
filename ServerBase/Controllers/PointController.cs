using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ServerBase.Entity;
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
    public class PointController : ControllerBase
    {
        private readonly ServerDbContext _accountDbContext;
        private readonly ServerDbContext _gameDbContext;
        private readonly ILogger<PointController> _logger;

        public PointController(ILogger<PointController> logger, ServerDbContext dbContext)
        {
            _logger = logger;
            _accountDbContext = dbContext;
            _gameDbContext = dbContext;
        }

        [Authorize]
        [HttpPost("gain")]
        public async Task<IActionResult> Gain([FromBody] PointGainRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var claim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
            {
                return BadRequest();
            }
            var id = int.Parse(claim.Value); //
            var pointType = Enum.Parse<PointType>(request.PointType); // ArgumentException

            var point = await _gameDbContext.Point.FirstOrDefaultAsync(i => i.Type == pointType);
            if (point == null)
            {
                point = new Point()
                {
                    UserId = id,
                    Type = pointType,
                    Quantity = 0,
                    CreateTime = DateTime.UtcNow,
                    Deleted = false,
                };
                await _gameDbContext.Point.AddAsync(point);
            }

            point.Quantity += request.Quantity;
            await _gameDbContext.SaveChangesAsync(); // 아직 트랜잭션이 필요하진 않아

            return Ok(point);
        }

        [Authorize]
        [HttpPost("steal")]
        public async Task<IActionResult> Steal([FromBody] PointStealRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var claim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
            {
                return BadRequest();
            }
            var id = int.Parse(claim.Value);
            var pointType = Enum.Parse<PointType>(request.PointType); // ArgumentException

            var targetPoint = await _gameDbContext.Point.FirstOrDefaultAsync(i => i.UserId == request.TargetUserId && i.Type == pointType);
            if (targetPoint == null)
            {
                return BadRequest();
            }

            var point = await _gameDbContext.Point.FirstOrDefaultAsync(i => i.UserId == id && i.Type == pointType);
            if (point == null)
            {
                point = new Point()
                {
                    UserId = id,
                    Type = pointType,
                    Quantity = 0,
                    CreateTime = DateTime.UtcNow,
                    Deleted = false,
                };
                await _gameDbContext.Point.AddAsync(point);
            }

            targetPoint.Quantity -= request.Quantity;
            point.Quantity += request.Quantity;
            await _gameDbContext.SaveChangesAsync(); // 동시성 문제가 발생 가능

            return Ok(new
            {
                point = point,
                targetPoint = targetPoint,
            });
        }
    }
}