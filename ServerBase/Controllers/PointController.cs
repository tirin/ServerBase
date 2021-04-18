using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServerBase.Entity;
using ServerBase.ServerTasks;
using System;
using System.Collections.Generic;
using System.Security.Claims;
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

        private class PointStealTask : ServerTask
        {
            private readonly ServerDbContext _gameDbContext;
            private readonly long _pointId;
            private readonly long _targetPointId;
            private readonly long _quantity;

            private Point _point;
            private Point _targetPoint;

            public PointStealTask(ServerDbContext gameDbContext, long pointId, long targetPointId, long quantity)
            {
                _gameDbContext = gameDbContext;
                _pointId = pointId;
                _targetPointId = targetPointId;
                _quantity = quantity;
            }

            public override IEnumerable<string> WaitingIds
            {
                get
                {
                    yield return _point.GetWaitingId();
                    yield return _targetPoint.GetWaitingId();
                }
            }

            public override async Task AssignAsync()
            {
                _point = await _gameDbContext.Point.FirstOrDefaultAsync(i => i.Id == _pointId);
                _targetPoint = await _gameDbContext.Point.FirstOrDefaultAsync(i => i.Id == _targetPointId);
            }

            protected override async Task<object[]> OnWorkAsync()
            {
                _targetPoint.Quantity -= _quantity;
                _point.Quantity += _quantity;
                await _gameDbContext.SaveChangesAsync(); // 동시성 문제가 발생 가능

                return new[]
                {
                    new
                    {
                        point = _point,
                        targetPoint = _targetPoint,
                    },
                };
            }
        }

        [Authorize]
        [HttpPost("steal")]
        public async Task<IActionResult> Steal([FromBody] PointStealRequest request, [FromServices] IServerTasksService service)
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

            var ret = await service.RunAsync(new PointStealTask(_gameDbContext, point.Id, targetPoint.Id, request.Quantity));
            return Ok(ret);
        }

        [Authorize]
        [HttpGet("all")]
        public async Task<IActionResult> All()
        {
            var points = await _gameDbContext.Point.ToArrayAsync();
            return Ok(points);
        }
    }
}