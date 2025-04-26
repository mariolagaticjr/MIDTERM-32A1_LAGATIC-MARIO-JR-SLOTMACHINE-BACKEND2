using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SlotMachineApi.Data;
using SlotMachineApi.DTOs;
using SlotMachineApi.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SlotMachineApi.Controllers
{
    [Route("api")]
    [ApiController]
    public class PlayersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PlayersController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        [HttpGet("users")]
        public async Task<IActionResult> GetRegisteredUsers()
        {
            var players = await _context.Players
                .Select(p => new PlayerDto
                {
                    StudentNumber = p.StudentNumber,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    RegistrationDate = p.RegistrationDate
                })
                .OrderByDescending(p => p.RegistrationDate)
                .ToListAsync();

            return Ok(players);
        }

       
        [HttpPost("register-user")]
        public async Task<IActionResult> RegisterUser([FromBody] PlayerDto playerDto)
        {
            if (playerDto == null)
            {
                return BadRequest("Player data is required.");
            }

            try
            {
               
                if (string.IsNullOrWhiteSpace(playerDto.StudentNumber) ||
                    !Regex.IsMatch(playerDto.StudentNumber, @"^C\d+$"))
                {
                    return BadRequest("Invalid student number format. Must start with 'C' followed by numbers only.");
                }

              
                if (string.IsNullOrWhiteSpace(playerDto.FirstName) || string.IsNullOrWhiteSpace(playerDto.LastName))
                {
                    return BadRequest("First name and last name are required.");
                }

               
                if (await _context.Players.AnyAsync(p => p.StudentNumber == playerDto.StudentNumber))
                {
                    return BadRequest("Student number already registered.");
                }

                var player = new Player
                {
                    StudentNumber = playerDto.StudentNumber,
                    FirstName = playerDto.FirstName,
                    LastName = playerDto.LastName,
                    RegistrationDate = DateTime.UtcNow
                };

                _context.Players.Add(player);

                _context.AuditLogs.Add(new AuditLog
                {
                    StudentNumber = player.StudentNumber,
                    Action = "Registration",
                    Status = "Success",
                    Timestamp = DateTime.UtcNow,
                    Details = $"User {player.FirstName} {player.LastName} registered"
                });

                await _context.SaveChangesAsync();

                return Ok(new { message = "User registered successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RegisterUser ERROR] {ex}");

                return StatusCode(500, "Registration failed: The server encountered an internal error. Please try again later.");
            }
        }

       
        [HttpGet("validate-player")]
        public async Task<IActionResult> ValidatePlayer([FromQuery] string studentNumber)
        {
            if (!Regex.IsMatch(studentNumber, @"^C\d+$"))
            {
                return Ok(new PlayerValidationDto
                {
                    IsValid = false,
                    Message = "Invalid student number format."
                });
            }

            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.StudentNumber == studentNumber);

            if (player == null)
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    StudentNumber = studentNumber,
                    Action = "Player Validation",
                    Status = "Error",
                    Timestamp = DateTime.UtcNow,
                    Details = "Student number not found"
                });

                await _context.SaveChangesAsync();

                return Ok(new PlayerValidationDto
                {
                    IsValid = false,
                    Message = "Student number not found."
                });
            }

            var recentGame = await _context.GameResults
                .Where(g => g.StudentNumber == studentNumber)
                .OrderByDescending(g => g.DatePlayed)
                .FirstOrDefaultAsync();

            if (recentGame != null && recentGame.DatePlayed > DateTime.UtcNow.AddHours(-3))
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    StudentNumber = studentNumber,
                    Action = "Player Validation",
                    Status = "Warning",
                    Timestamp = DateTime.UtcNow,
                    Details = "Attempted to play within 3-hour cooldown period"
                });

                await _context.SaveChangesAsync();

                var nextPlayTime = recentGame.DatePlayed.AddHours(3);
                var timeRemaining = nextPlayTime - DateTime.UtcNow;

                return Ok(new PlayerValidationDto
                {
                    IsValid = false,
                    Message = $"You can play again in {timeRemaining.Hours} hours and {timeRemaining.Minutes} minutes."
                });
            }

            _context.AuditLogs.Add(new AuditLog
            {
                StudentNumber = studentNumber,
                Action = "Player Validation",
                Status = "Success",
                Timestamp = DateTime.UtcNow,
                Details = $"Player {player.FullName} validated successfully"
            });

            await _context.SaveChangesAsync();

            return Ok(new PlayerValidationDto
            {
                IsValid = true,
                StudentName = player.FullName,
                Message = "Player validated successfully."
            });
        }
    }
}
