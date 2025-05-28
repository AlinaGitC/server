using Microsoft.AspNetCore.Mvc;
using server.DTOs;
using server.Services;
using server.Models;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        public UserController(UserService userService) { _userService = userService; }

        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            var userId = await _userService.RegisterAsync(dto);
            if (userId > 0)
                return Ok(new { id = userId });
            return BadRequest("Логин уже занят");
        }

        /// <summary>
        /// Получить всех пользователей
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<UserDto>>> GetAll()
        {
            var users = await _userService.GetAllAsync();
            var dtos = users.Select(u => new UserDto
            {
                Id = u.ID,
                Login = u.Login,
                Firstname = u.Firstname,
                MiddleName = u.MiddleName,
                LastName = u.LastName,
                Email = u.Email,
                Phone = u.Phone,
                Avatar = u.Avatar,
                IsActive = u.IsActive
            }).ToList();
            return Ok(dtos);
        }

        /// <summary>
        /// Аутентификация пользователя
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login([FromBody] LoginUserDto dto)
        {
            var user = await _userService.AuthenticateAsync(dto);
            if (user == null) return Unauthorized("Неверный логин или пароль");
            var dtoUser = new UserDto
            {
                Id = user.ID,
                Login = user.Login,
                Firstname = user.Firstname,
                MiddleName = user.MiddleName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Avatar = user.Avatar,
                IsActive = user.IsActive
            };
            return Ok(dtoUser);
        }

        /// <summary>
        /// Получить аватар пользователя
        /// </summary>
        [HttpGet("{id}/avatar")]
        public async Task<IActionResult> GetAvatar(int id)
        {
            var user = (await _userService.GetAllAsync()).FirstOrDefault(u => u.ID == id);
            if (user == null || user.Avatar == null)
                return NotFound();
            return File(user.Avatar, "image/png"); // или image/jpeg, если нужно
        }

        /// <summary>
        /// Обновить аватар пользователя
        /// </summary>
        [HttpPut("{id}/avatar")]
        public async Task<IActionResult> UpdateAvatar(int id, [FromBody] AvatarDto dto)
        {
            var user = (await _userService.GetAllAsync()).FirstOrDefault(u => u.ID == id);
            if (user == null)
                return NotFound();
            user.Avatar = dto.Avatar;
            await _userService.UpdateUserAsync(user);
            return Ok();
        }

        /// <summary>
        /// Обновить профиль пользователя
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] UserDto dto)
        {
            var user = (await _userService.GetAllAsync()).FirstOrDefault(u => u.ID == id);
            if (user == null)
                return NotFound();
            user.Firstname = dto.Firstname;
            user.MiddleName = dto.MiddleName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;
            user.Phone = dto.Phone;
            // user.Avatar не трогаем здесь
            await _userService.UpdateUserAsync(user);
            return Ok();
        }
    }
} 