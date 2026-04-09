using System;

namespace CST2550
{
	// Represents a user account stored in the Users table
	public class AuthRecord
	{
		public int Id { get; set; }
		public string Username { get; set; }
		public string PasswordHash { get; set; }
		public string FullName { get; set; }
		public string Role { get; set; }
		public DateTime CreatedAt { get; set; }

		public AuthRecord() { }

		public AuthRecord(string username, string passwordHash, string fullName, string role = "Operator")
		{
			Username = username;
			PasswordHash = passwordHash;
			FullName = fullName;
			Role = role;
			CreatedAt = DateTime.UtcNow;
		}
	}

	// The data the frontend sends when a user tries to log in
	public class LoginRequest
	{
		public string Username { get; set; }
		public string Password { get; set; }
	}

	// The data the frontend sends when creating a new account
	public class RegisterRequest
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public string FullName { get; set; }
	}

	// What we send back to the frontend after login or register
	public class AuthResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public string Username { get; set; }
		public string FullName { get; set; }
		public string Role { get; set; }
		public string Token { get; set; }
	}
}