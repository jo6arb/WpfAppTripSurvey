using System;

namespace WpfAppTrip.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public DateTime RegistrationDate { get; set; }
        
        // Добавляем свойства для имени, фамилии и отчества
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }

        public bool IsAdmin => Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) ?? false;
        
        // Получаем имя и фамилию из Username, если они не заданы явно
        public string GetFirstName()
        {
            if (!string.IsNullOrEmpty(FirstName))
                return FirstName;
                
            var parts = Username?.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return parts?.Length > 1 ? parts[1] : Username;
        }
        
        public string GetLastName()
        {
            if (!string.IsNullOrEmpty(LastName))
                return LastName;
                
            var parts = Username?.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return parts?.Length > 0 ? parts[0] : string.Empty;
        }
        
        public string GetMiddleName()
        {
            if (!string.IsNullOrEmpty(MiddleName))
                return MiddleName;
                
            var parts = Username?.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return parts?.Length > 2 ? parts[2] : string.Empty;
        }
        
        // Полное имя пользователя
        public string GetFullName()
        {
            return $"{GetLastName()} {GetFirstName()} {GetMiddleName()}".Trim();
        }
    }
} 