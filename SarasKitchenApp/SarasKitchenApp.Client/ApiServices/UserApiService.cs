using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.JSInterop;
using Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace SarasKitchenApp.Client.ApiServices
{
    public class UserApiService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly ILogger<UserApiService> _logger;

        public UserApiService(IJSRuntime jsRuntime, ILogger<UserApiService> logger)
        {
            _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> AuthenticateAsync(UserLoginModel user)
        {
            try
            {
                var responseData = await _jsRuntime.InvokeAsync<AuthenticationResponse>(
                    "apiClient.post",
					"user/authenticate",
					user
                );

                if (responseData is null || string.IsNullOrWhiteSpace(responseData.Token))
                {
                    throw new InvalidOperationException("Login failed: No token received.");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now:dd MMM yyyy HH:mm:ss} " +
                    $"[ERROR] AuthenticateAsync :: An error occurred during authentication.\n{ex}");
                throw;
            }
        }

        public async Task<User> CreateUserAsync(UserSignUpModel user)
        {
            try
            {
                var responseData = await _jsRuntime.InvokeAsync<User>(
                    "apiClient.post",
                    "user",
                    user
                );

                return responseData is null ? throw new InvalidOperationException("Failed to create user.") : responseData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now:dd MMM yyyy HH:mm:ss} " +
                    $"[ERROR] CreateUserAsync :: An error occurred while creating the user {user.Username}.\n{ex}");
                throw;
            }
        }

        public async Task<User> GetUserByIdAsync(string id)
        {
            try
            {
				var responseData = await _jsRuntime.InvokeAsync<User>(
					"apiClient.get",
					$"user/{id}"
			    );

				return responseData is null ? throw new InvalidOperationException("Failed to get user.") : responseData;
			}
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now:dd MMM yyyy HH:mm:ss} " +
                    $"[ERROR] GetUserByIdAsync :: An error occurred while fetching user with ID {id}.\n{ex}");
                throw;
            }
        }

        public async Task<UserInfo> GetUserInfoAsync()
        {
            try
            {
                var responseData = await _jsRuntime.InvokeAsync<UserInfo>(
                    "apiClient.get",
                    "user/me"
                );

                return responseData is null ? throw new InvalidOperationException("Failed to get user info.") : responseData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now:dd MMM yyyy HH:mm:ss} " +
                    $"[ERROR] GetUserInfoAsync :: An error occurred while fetching user info.\n{ex}");
                throw;
            }
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
				var responseData = await _jsRuntime.InvokeAsync<bool>(
					"apiClient.post",
					"user/logout"
				);

				return responseData is false ? throw new InvalidOperationException("Failed to logout user.") : responseData;
			}
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now:dd MMM yyyy HH:mm:ss} " +
                    $"[ERROR] LogoutAsync :: An error occurred while trying to log user out.\n{ex}");
                return false;
            }
        }


        public class AuthenticationResponse
        {
            public string Token { get; set; } = string.Empty;
        }
    }
}
