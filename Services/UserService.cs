using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Models;
using Repositories;
using System.Security.Cryptography;
using System.Text;

namespace Services
{
    public interface IUserService
    {
        Task<List<User>> GetAllAsync();
        Task<User?> GetOneByIdAsync(string id);
        Task<User> CreateUserAsync(UserSignUpModel user);
        Task<User?> AuthenticateUserAsync(string username, string password);
    }


    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;
        private readonly PasswordHasher<object> _passwordHasher;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            this._userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _passwordHasher = new PasswordHasher<object>();
        }

        public async Task<User?> AuthenticateUserAsync(string username, string password)
        {
            try
            {
                var user = await _userRepository.GetUserByUsernameAsync(username);
                if (user is null || !VerifyPassword(password, user.PasswordHash))
                {
                    throw new NullReferenceException($"User with username {username} could not be authenticated.");
                }
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] AuthenticateUserAsync :: An error occured while authenticating user with username {username}.\n{ex}");
                throw;
            }
        }

        public async Task<List<User>> GetAllAsync()
        {
            try
            {
                return await _userRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetAllAsync :: An error occured while fetching all Users.\n{ex}");
                throw;
            }
        }

        public async Task<User?> GetOneByIdAsync(string id)
        {
            try
            {
                return await _userRepository.GetOneByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetOneByIdAsync :: An error occured while fetching User with id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<User> CreateUserAsync(UserSignUpModel userSignUp)
        {
            try
            {
                User user = new()
                {
                    Role = "User",
                    Username = userSignUp.Username,
                    PasswordHash = HashPassword(userSignUp.Password)
                };

                user.Validate();

                await _userRepository.InsertAsync(user);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] CreateUserAsync :: An error occured while adding new User with username {userSignUp.Username}.\n{ex}");
                throw;
            }
        }

        public string HashPassword(string password)
        {
            try
            {
                return _passwordHasher.HashPassword(null!, password);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] HashPassword :: An error occured while trying to hash password.\n{ex}");
                throw;
            }
        }

        public bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                var result = _passwordHasher.VerifyHashedPassword(null!, storedHash, password);
                return result == PasswordVerificationResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] VerifyPassword :: An error occured while trying to verify password.\n{ex}");
                throw;
            }

        }
    }
}
