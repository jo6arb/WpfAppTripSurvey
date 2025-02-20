using System;

namespace WpfAppTrip.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime RegistrationDate { get; set; }

        public bool IsAdmin => Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) ?? false;
    }
} 