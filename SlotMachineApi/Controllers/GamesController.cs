using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SlotMachineApi.Data;
using SlotMachineApi.DTOs;
using SlotMachineApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlotMachineApi.Controllers
{
    [Route("api")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GamesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/games
        [HttpGet("games")]
        public async Task<IActionResult> GetGames([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            // Adjust endDate to include the entire day
            endDate = endDate.Date.AddDays(1).AddTicks(-1);

            var games = await _context.GameResults
                .Include(g => g.Player)
                .Where(g => g.DatePlayed >= startDate && g.DatePlayed <= endDate)
                .OrderByDescending(g => g.DatePlayed)
                .Select(g => new GameResultDto
                {
                    Id = g.Id,
                    StudentNumber = g.StudentNumber,
                    StudentName = g.Player.FullName,
                    Result = g.Result,
                    RetryCount = g.RetryCount,
                    DatePlayed = g.DatePlayed
                })
                .ToListAsync();

            return Ok(games);
        }

        // GET: api/winners
        [HttpGet("winners")]
        public async Task<IActionResult> GetWinners([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            // Adjust endDate to include the entire day
            endDate = endDate.Date.AddDays(1).AddTicks(-1);

            var winners = await _context.GameResults
                .Include(g => g.Player)
                .Where(g =>
                    g.DatePlayed >= startDate &&
                    g.DatePlayed <= endDate &&
                    g.Result == "win")
                .OrderByDescending(g => g.DatePlayed)
                .Select(g => new GameResultDto
                {
                    Id = g.Id,
                    StudentNumber = g.StudentNumber,
                    StudentName = g.Player.FullName,
                    Result = g.Result,
                    RetryCount = g.RetryCount,
                    DatePlayed = g.DatePlayed
                })
                .ToListAsync();

            return Ok(winners);
        }

        // GET: api/recent-players
        [HttpGet("recent-players")]
        public async Task<IActionResult> GetRecentPlayers()
        {
            var threeHoursAgo = DateTime.UtcNow.AddHours(-3);

            var recentPlayers = await _context.GameResults
                .Include(g => g.Player)
                .Where(g => g.DatePlayed >= threeHoursAgo)
                .OrderByDescending(g => g.DatePlayed)
                .Select(g => new
                {
                    Id = g.Id,
                    StudentNumber = g.StudentNumber,
                    StudentName = g.Player.FullName,
                    DatePlayed = g.DatePlayed
                })
                .ToListAsync();

            // Group by student number to get only the most recent play for each student
            var uniquePlayers = recentPlayers
                .GroupBy(p => p.StudentNumber)
                .Select(g => g.OrderByDescending(p => p.DatePlayed).First())
                .ToList();

            return Ok(uniquePlayers);
        }

        // POST: api/save-game
        [HttpPost("save-game")]
        public async Task<IActionResult> SaveGame(SaveGameDto saveGameDto)
        {
            // Validate that the student exists
            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.StudentNumber == saveGameDto.StudentNumber);

            if (player == null)
            {
                return BadRequest("Student not found.");
            }

            // Create the game result record
            var gameResult = new GameResult
            {
                StudentNumber = saveGameDto.StudentNumber,
                Result = saveGameDto.Result,
                RetryCount = saveGameDto.RetryCount,
                DatePlayed = saveGameDto.DatePlayed
            };

            _context.GameResults.Add(gameResult);

            // Add audit log entry
            _context.AuditLogs.Add(new AuditLog
            {
                StudentNumber = saveGameDto.StudentNumber,
                Action = "Game Played",
                Status = saveGameDto.Result == "win" ? "Success" : "Info",
                Timestamp = DateTime.UtcNow,
                Details = $"Player {player.FullName} {saveGameDto.Result} with {saveGameDto.RetryCount} retries"
            });

            await _context.SaveChangesAsync();

            return Ok(new { message = "Game result saved successfully" });
        }

        // GET: api/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetGameStats()
        {
            var totalPlayers = await _context.Players.CountAsync();
            var totalGames = await _context.GameResults.CountAsync();
            var totalWins = await _context.GameResults.CountAsync(g => g.Result == "win");

            double winRate = 0;
            if (totalGames > 0)
            {
                winRate = Math.Round((double)totalWins / totalGames * 100, 2);
            }

            var stats = new GameStatsDto
            {
                TotalPlayers = totalPlayers,
                TotalGames = totalGames,
                TotalWins = totalWins,
                WinRate = winRate
            };

            return Ok(stats);
        }

        // GET: api/audit
        [HttpGet("audit")]
        public async Task<IActionResult> GetAuditTrail([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            endDate = endDate.Date.AddDays(1).AddTicks(-1);

            var auditLogs = await _context.AuditLogs
                .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
                .OrderByDescending(a => a.Timestamp)
                .Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    StudentNumber = a.StudentNumber,
                    Action = a.Action,
                    Status = a.Status,
                    Timestamp = a.Timestamp,
                    Details = a.Details
                })
                .ToListAsync();

            return Ok(auditLogs);
        }
    }
}
