using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;

namespace CST2550.Controllers
{
	// Handles login and registration requests from the frontend
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly DatabaseManager _db;

		public AuthController(DatabaseManager db)
		{
			_db = db;
		}

		// POST /api/auth/login
		[HttpPost("login")]
		public IActionResult Login([FromBody] LoginRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
				return BadRequest(new AuthResponse { Success = false, Message = "Username and password are required." });

			var user = _db.GetUserByUsername(request.Username.Trim());

			// Return 401 whether the username doesn't exist or the password is wrong -
			// don't tell them which one it is for security reasons
			if (user == null || !DatabaseManager.VerifyPassword(request.Password, user.PasswordHash))
				return Unauthorized(new AuthResponse { Success = false, Message = "Invalid username or password." });

			// Generate a simple token by base64 encoding the username + current timestamp.
			// This isn't proper JWT but it works for this project.
			string rawToken = $"{user.Username}:{DateTime.UtcNow.Ticks}";
			string token = Convert.ToBase64String(Encoding.UTF8.GetBytes(rawToken));

			return Ok(new AuthResponse
			{
				Success = true,
				Message = "Login successful.",
				Username = user.Username,
				FullName = user.FullName,
				Role = user.Role,
				Token = token
			});
		}

		// POST /api/auth/register
		[HttpPost("register")]
		public IActionResult Register([FromBody] RegisterRequest request)
		{
			// Basic validation
			if (string.IsNullOrWhiteSpace(request.Username) ||
				string.IsNullOrWhiteSpace(request.Password) ||
				string.IsNullOrWhiteSpace(request.FullName))
				return BadRequest(new AuthResponse { Success = false, Message = "All fields are required." });

			if (request.Password.Length < 6)
				return BadRequest(new AuthResponse { Success = false, Message = "Password must be at least 6 characters." });

			if (_db.UsernameExists(request.Username.Trim()))
				return Conflict(new AuthResponse { Success = false, Message = "That username is already taken." });

			var newUser = new AuthRecord(
				request.Username.Trim(),
				DatabaseManager.HashPassword(request.Password),
				request.FullName.Trim()
			);

			_db.CreateUser(newUser);

			return Ok(new AuthResponse
			{
				Success = true,
				Message = "Account created successfully.",
				Username = newUser.Username,
				FullName = newUser.FullName,
				Role = newUser.Role
			});
		}
	}
}
