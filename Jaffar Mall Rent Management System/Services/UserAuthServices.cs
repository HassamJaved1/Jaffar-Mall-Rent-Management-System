
using Jaffar_Mall_Rent_Management_System.Backend_Logics;
using Jaffar_Mall_Rent_Management_System.Models;
using Jaffar_Mall_Rent_Management_System.Utilities;

namespace Jaffar_Mall_Rent_Management_System.Services
{
    public class UserAuthService
    {
        private readonly UserAuthRepository _userAuthRepository;

        public UserAuthService(UserAuthRepository userAuthRepository)
        {
            _userAuthRepository = userAuthRepository;
        }

        /// <summary>
        /// Authenticate a user by username and password.
        /// Returns user ID if successful, otherwise null.
        /// </summary>
        public async Task<BackendResponse<long>> AuthenticateUserAsync(string username, string password)
        {
            var response = new BackendResponse<long>();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return new BackendResponse<long>
                {
                    Message = "Username and password cannot be empty.",
                    Data = 0,
                    Code = 400
                };

            try
            {
                var user = await _userAuthRepository.GetDetailsByUsernameAsync(username);

                if (user is null || string.IsNullOrEmpty(user.Password))
                    return new BackendResponse<long>
                    {
                        Message = "Unable to find user with the added username.",
                        Data = 0,
                        Code = 400
                    };

                bool isValid = PasswordHasher.VerifyPassword(password, user.Password);

                return new BackendResponse<long>
                {
                    Message = isValid ? "Authentication successful." : "Invalid username or password.",
                    Data = isValid ? user.Id : 0,
                    Code = isValid ? 200 : 401
                };
            }
            catch (Exception ex)
            {
                // Ideally use ILogger here
                Console.WriteLine($"Error authenticating user '{username}': {ex.Message}");

                return new BackendResponse<long>
                {
                    Message = "An error occurred while authenticating.",
                    Data = 0,
                    Code = 500
                };
            }
        }


        /// <summary>
        /// Reset password for a user.
        /// Returns true if successful, false otherwise.
        /// </summary>
        public async Task<BackendResponse<bool>> ResetPasswordAsync(string username, string oldPassword, string newPassword)
        {
            var response = new BackendResponse<bool>();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                return new BackendResponse<bool>
                {
                    Message = "Username, old password, and new password cannot be empty.",
                    Data = false,
                    Code = 400
                };
            }

            try
            {
                // Authenticate user first
                var authResult = await AuthenticateUserAsync(username, oldPassword);
                if (authResult.Data == 0) 
                    return new BackendResponse<bool> // Invalid credentials
                    {
                        Message = "The username or password is incorrect",
                        Data = false,
                        Code = 400
                    }; 

                var rowsAffected = await _userAuthRepository.UpdatePasswordAsync(authResult.Data, PasswordHasher.HashPassword(newPassword));

                return new BackendResponse<bool> // Invalid credentials
                {
                    Message = rowsAffected > 0 ? "The password has been updated successfully" : "Unable to update password",
                    Data = rowsAffected > 0 ? true : false,
                    Code = rowsAffected > 0 ? 200 : 500
                };
            }
            catch (Exception ex)
            {
                // Use proper logging in production
                Console.WriteLine($"Error resetting password for '{username}': {ex.Message}");
                return new BackendResponse<bool>
                {
                    Message = "An error occurred while resetting password.",
                    Data = false,
                    Code = 500
                };
            }
        }
    }
}
