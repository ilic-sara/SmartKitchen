using Microsoft.Extensions.Logging;
using Moq;
using Services;
using Models;
using Repositories;

namespace Tests.Unit.Services
{
    public class UserServiceTests
    {
        private readonly UserService _service;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ILogger<UserService>> _loggerMock;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _loggerMock = new Mock<ILogger<UserService>>();

            _service = new UserService(_userRepositoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllUsers()
        {
            var users = new List<User>
            {
                new() { Id = "1", Username = "Admin1", Role = "Admin" },
                new() { Id = "2", Username = "Admin2", Role = "Admin" }
            };

            _userRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(users);

            var result = await _service.GetAllAsync();

            Assert.Equal(users, result);

            _userRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetOneByIdAsync_ShouldReturnUser_WhenExists()
        {
            var user = new User { Id = "1", Username = "Admin1", Role = "Admin" };
            _userRepositoryMock.Setup(repo => repo.GetOneByIdAsync("1")).ReturnsAsync(user);

            var result = await _service.GetOneByIdAsync("1");

            Assert.NotNull(result);
            Assert.Equal(user, result);

            _userRepositoryMock.Verify(repo => repo.GetOneByIdAsync("1"), Times.Once);
        }

        [Fact]
        public async Task GetOneByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            _userRepositoryMock.Setup(repo => repo.GetOneByIdAsync("1")).ReturnsAsync((User?)null);

            var result = await _service.GetOneByIdAsync("1");

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldCreateUserSuccessfully()
        {
            var userSignUpModel = new UserSignUpModel()
            {
                Username = "NewUser",
                Password = "SecurePassword123",
                ConfirmPassword = "SecurePassword123"
            };
            var result = await _service.CreateUserAsync(userSignUpModel);

            Assert.NotNull(result);
            Assert.Equal("Admin", result.Role);
            Assert.Equal(userSignUpModel.Username, result.Username);
            Assert.False(string.IsNullOrEmpty(result.PasswordHash));

            _userRepositoryMock.Verify(repo => repo.InsertAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrowException_WhenRepositoryFails()
        {
            var userSignUpModel = new UserSignUpModel()
            {
                Username = "NewUser",
                Password = "SecurePassword123",
                ConfirmPassword = "SecurePassword123"
            };

            _userRepositoryMock.Setup(repo => repo.InsertAsync(It.IsAny<User>()))
                         .ThrowsAsync(new Exception("Database error"));

            var exception = await Assert.ThrowsAsync<Exception>(() => _service.CreateUserAsync(userSignUpModel));
            Assert.Contains("Database error", exception.Message);
        }

        [Fact]
        public async Task AuthenticateUserAsync_ShouldReturnUser_WhenCredentialsAreCorrect()
        {
            var user = new User { Id = "1", Username = "Admin1", PasswordHash = _service.HashPassword("SecurePass123"), Role = "Admin" };
            _userRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync("Admin1")).ReturnsAsync(user);

            var result = await _service.AuthenticateUserAsync("Admin1", "SecurePass123");

            Assert.NotNull(result);
            Assert.Equal(user, result);
            _userRepositoryMock.Verify(repo => repo.GetUserByUsernameAsync("Admin1"), Times.Once);
        }

        [Fact]
        public async Task AuthenticateUserAsync_ShouldThrowException_WhenUserNotFound()
        {
            _userRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync("Admin1")).ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<NullReferenceException>(() => _service.AuthenticateUserAsync("Admin1", "SecurePass123"));
        }

        [Fact]
        public async Task AuthenticateUserAsync_ShouldThrowException_WhenPasswordIsIncorrect()
        {
            var user = new User { Id = "1", Username = "Admin1", PasswordHash = _service.HashPassword("CorrectPass"), Role = "Admin" };
            _userRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync("Admin1")).ReturnsAsync(user);

            await Assert.ThrowsAsync<NullReferenceException>(() => _service.AuthenticateUserAsync("Admin1", "WrongPass"));
        }

        [Fact]
        public void HashPassword_ShouldReturnHashedString()
        {
            var password = "SecurePass123";

            var result = _service.HashPassword(password);

            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.NotEqual(password, result); 
        }

        [Fact]
        public void VerifyPassword_ShouldReturnTrue_WhenPasswordMatchesHash()
        {
            var password = "SecurePass123";
            var hashedPassword = _service.HashPassword(password);

            var result = _service.VerifyPassword(password, hashedPassword);

            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalse_WhenPasswordDoesNotMatchHash()
        {
            var hashedPassword = _service.HashPassword("CorrectPass");

            var result = _service.VerifyPassword("WrongPass", hashedPassword);

            Assert.False(result);
        }
    }
}
