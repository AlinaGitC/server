using server.Models;
using server.DTOs;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace server.Services
{
    /// <summary>
    /// Сервис для работы с пользователями (регистрация, аутентификация)
    /// </summary>
    public class UserService
    {
        private readonly AppDbContext _context;
        public UserService(AppDbContext context) { _context = context; }

        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        public async Task<int> RegisterAsync(RegisterUserDto dto)
        {
            if (await _context.Userr.AnyAsync(u => u.Login == dto.Login)) return 0; // Логин уже занят
            var user = new Userr
            {
                Login = dto.Login,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Firstname = dto.Firstname,
                LastName = dto.LastName,
                MiddleName = dto.MiddleName,
                Email = dto.Email,
                Phone = dto.Phone,
                BirthDate = dto.BirthDate,
                IsActive = true
            };
            _context.Userr.Add(user);
            await _context.SaveChangesAsync();
            return user.ID;
        }

        /// <summary>
        /// Аутентификация пользователя
        /// </summary>
        public async Task<Userr?> AuthenticateAsync(LoginUserDto dto)
        {
            var user = await _context.Userr.FirstOrDefaultAsync(u => u.Login == dto.Login);
            if (user == null) return null;
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password)) return null;
            return user;
        }

        /// <summary>
        /// Получить всех пользователей
        /// </summary>
        public async Task<List<Userr>> GetAllAsync()
        {
            return await _context.Userr.ToListAsync();
        }

        /// <summary>
        /// Обновить пользователя (аватар или профиль)
        /// </summary>
        public async Task UpdateUserAsync(Userr user)
        {
            _context.Userr.Update(user);
            await _context.SaveChangesAsync();
        }
    }
} 