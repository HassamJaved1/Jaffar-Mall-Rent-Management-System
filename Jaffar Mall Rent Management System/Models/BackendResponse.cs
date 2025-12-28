using Microsoft.AspNetCore.Mvc;

namespace Jaffar_Mall_Rent_Management_System.Models
{
    public class BackendResponse<T>
    {
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public int Code { get; set; }

        // Helper for Success
        public static BackendResponse<T> Success(T data, string message = "Request successful.")
            => new() { Data = data, Message = message, Code = 200 };

        // Helper for Failure
        public static BackendResponse<T> Failure(string message, int code = 400)
            => new() { Data = default, Message = message, Code = code };
    }

    public static class ApiResponseExtensions
    {
        public static ObjectResult ToActionResult<T>(this BackendResponse<T> response)
        {
            // This physically sets the HTTP Status Code to match your internal Code
            return new ObjectResult(response) { StatusCode = response.Code };
        }
    }
}
