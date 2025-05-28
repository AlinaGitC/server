namespace server.DTOs
{
    /// <summary>
    /// DTO для входа пользователя
    /// </summary>
    public class LoginUserDto
    {
        public string Login { get; set; } // Логин пользователя
        public string Password { get; set; } // Пароль
    }
} 