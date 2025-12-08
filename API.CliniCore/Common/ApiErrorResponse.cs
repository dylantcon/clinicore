using System;
using System.Collections.Generic;

namespace API.CliniCore.Common
{
    /// <summary>
    /// Standardized error response for API errors>
    /// </summary>
    public class ApiErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
        public string? TraceId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiErrorResponse FromMessage(string message)
        {
            return new ApiErrorResponse { Message = message };
        }

        public static ApiErrorResponse FromErrors(string message, IEnumerable<string> errors)
        {
            return new ApiErrorResponse
            {
                Message = message,
                Errors = new List<string>(errors)
            };
        }
    }
}
