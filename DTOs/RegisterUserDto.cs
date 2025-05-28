namespace server.DTOs
{
    /// <summary>
    /// DTO для регистрации пользователя
    /// </summary>
    public class RegisterUserDto
    {
        public string Login { get; set; } // Логин пользователя
        public string Password { get; set; } // Пароль
        public string Firstname { get; set; } // Имя
        public string LastName { get; set; } // Фамилия
        public string Email { get; set; } // Email
        public string Phone { get; set; } // Телефон
        public string MiddleName { get; set; } // Отчество
        public DateTime? BirthDate { get; set; } // Дата рождения
    }
} 