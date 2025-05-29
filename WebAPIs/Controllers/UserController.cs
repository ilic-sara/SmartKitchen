using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services;
using System.Security.Claims;

namespace WebAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly JwtTokenService _jwtTokenService;
        private readonly ILogger<UserController> _logger;
        private readonly IConfiguration _configuration;
        private readonly int _tokenLifetimeInHours;

        public UserController(IUserService userService, 
                              JwtTokenService jwtTokenService, 
                              ILogger<UserController> logger,
                              IConfiguration configuration)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _tokenLifetimeInHours = _configuration.GetValue<int>("Jwt:TokenLifetimeInHours", 1);

        }

        [HttpPost("authenticate")]
        public async Task<ActionResult<string>> Authenticate([FromBody] UserLoginModel loginRequest)
        {
            try
            {
                var user = await _userService.AuthenticateUserAsync(loginRequest.Username, loginRequest.Password);
                if (user is null)
                {
                    return Unauthorized("Authentication failed.");
                }

                var token = _jwtTokenService.GenerateToken(user);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddHours(_tokenLifetimeInHours)
                };

                Response.Cookies.Append("authToken", token, cookieOptions);

                return Ok(new { Token = token });
            }
            catch(Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] Authenticate :: An error occurred while trying to authenticate user.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<User>> CreateUser([FromBody] UserSignUpModel user)
        {
            try
            {
                var createdUser = await _userService.CreateUserAsync(user);
                var token = _jwtTokenService.GenerateToken(createdUser);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddHours(_tokenLifetimeInHours)
                };

                Response.Cookies.Append("authToken", token, cookieOptions);
                return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] CreateUser :: An error occurred while trying to create user.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("logout")]
        [Authorize(Roles = "Admin")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("authToken"); 
            return Ok(true);
        }

        [HttpGet("me")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetUserInfo()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.Identity?.Name;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(username))
            {
                return Unauthorized();
            }

            return Ok(new UserInfo
            {
                Id = userId,
                Username = username,
                Role = role ?? ""
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(string id)
        {
            try
            {
                var user = await _userService.GetOneByIdAsync(id);
                if (user is null)
                    return NotFound();
                return Ok(user);
            }
            catch (Exception ex) 
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetUserById :: An error occurred while trying to get user with id {id}.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<User>>> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetAllUsers :: An error occurred while trying to get all users.\n{ex}\n");
                return StatusCode(500, "Internal server error");
            }
            
        }
    }

    

}
