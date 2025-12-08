using API.CliniCore.Common;
using Core.CliniCore.Api;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.DTOs.Auth;
using Core.CliniCore.Mapping;
using Core.CliniCore.Repositories;
using Core.CliniCore.Requests.Auth;
using Core.CliniCore.Service;
using Microsoft.AspNetCore.Mvc;

namespace API.CliniCore.Controllers
{
    [ApiController]
    [Route(ApiRoutes.Auth.BasePath)]
    public class AuthController : ControllerBase
    {
        private readonly ICredentialRepository _credentialRepository;

        public AuthController(ICredentialRepository credentialRepository)
        {
            _credentialRepository = credentialRepository;
        }

        /// <summary>
        /// Get all credentials (excludes password hashes)
        /// </summary>
        [HttpGet]
        public ActionResult<IEnumerable<UserCredentialDto>> GetAll()
        {
            var credentials = _credentialRepository.GetAll();
            var dtos = credentials.Select(c => c.ToDto());
            return Ok(dtos);
        }

        /// <summary>
        /// Get credential by ID
        /// </summary>
        [HttpGet(ApiRoutes.Auth.ById)]
        public ActionResult<UserCredentialDto> GetById(Guid id)
        {
            var credential = _credentialRepository.GetById(id);
            if (credential == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Credential with ID {id} not found"));
            }
            return Ok(credential.ToDto());
        }

        /// <summary>
        /// Get credential by username
        /// </summary>
        [HttpGet(ApiRoutes.Auth.ByUsername)]
        public ActionResult<UserCredentialDto> GetByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest(ApiErrorResponse.FromMessage("Username is required"));
            }

            var credential = _credentialRepository.GetByUsername(username);
            if (credential == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Credential with username '{username}' not found"));
            }
            return Ok(credential.ToDto());
        }

        /// <summary>
        /// Search credentials by username
        /// </summary>
        [HttpGet(ApiRoutes.Auth.Search)]
        public ActionResult<IEnumerable<UserCredentialDto>> Search([FromQuery] string q)
        {
            var credentials = _credentialRepository.Search(q ?? "");
            var dtos = credentials.Select(c => c.ToDto());
            return Ok(dtos);
        }

        /// <summary>
        /// Validate credentials (login)
        /// </summary>
        [HttpPost(ApiRoutes.Auth.Validate)]
        public ActionResult<UserCredentialDto> Validate([FromBody] ValidateCredentialRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiErrorResponse.FromErrors("Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            var credential = _credentialRepository.GetByUsername(request.Username);
            if (credential == null)
            {
                return Unauthorized(ApiErrorResponse.FromMessage("Invalid username or password"));
            }

            if (!BasicAuthenticationService.VerifyPassword(request.Password, credential.PasswordHash))
            {
                return Unauthorized(ApiErrorResponse.FromMessage("Invalid username or password"));
            }

            // Update last login time
            credential.LastLoginAt = DateTime.UtcNow;
            _credentialRepository.Update(credential);

            return Ok(credential.ToDto());
        }

        /// <summary>
        /// Register new credentials
        /// </summary>
        [HttpPost(ApiRoutes.Auth.Register)]
        public ActionResult<UserCredentialDto> Register([FromBody] RegisterCredentialRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiErrorResponse.FromErrors("Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            // Check if username already exists
            if (_credentialRepository.Exists(request.Username))
            {
                return Conflict(ApiErrorResponse.FromMessage($"Username '{request.Username}' is already taken"));
            }

            // Validate role
            var validRoles = new[] { "Patient", "Physician", "Administrator" };
            if (!validRoles.Contains(request.Role, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(ApiErrorResponse.FromMessage(
                    $"Invalid role. Must be one of: {string.Join(", ", validRoles)}"));
            }

            var credential = new UserCredential
            {
                Id = request.Id,
                Username = request.Username,
                PasswordHash = BasicAuthenticationService.HashPassword(request.Password),
                Role = request.Role,
                CreatedAt = DateTime.UtcNow
            };

            _credentialRepository.Add(credential);

            return CreatedAtAction(nameof(GetById), new { id = credential.Id }, credential.ToDto());
        }

        /// <summary>
        /// Change password
        /// </summary>
        [HttpPut(ApiRoutes.Auth.ChangePassword)]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiErrorResponse.FromErrors("Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            var credential = _credentialRepository.GetByUsername(request.Username);
            if (credential == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"User '{request.Username}' not found"));
            }

            // Verify current password
            if (!BasicAuthenticationService.VerifyPassword(request.CurrentPassword, credential.PasswordHash))
            {
                return Unauthorized(ApiErrorResponse.FromMessage("Current password is incorrect"));
            }

            // Update password
            credential.PasswordHash = BasicAuthenticationService.HashPassword(request.NewPassword);
            _credentialRepository.Update(credential);

            return NoContent();
        }

        /// <summary>
        /// Update a credential (e.g., LastLoginAt)
        /// </summary>
        [HttpPut("{id}")]
        public IActionResult Update(Guid id, [FromBody] UserCredentialDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(ApiErrorResponse.FromMessage("ID in URL does not match ID in body"));
            }

            var existing = _credentialRepository.GetById(id);
            if (existing == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Credential with ID {id} not found"));
            }

            // Update allowed fields (not password hash - use change-password for that)
            existing.LastLoginAt = dto.LastLoginAt;

            _credentialRepository.Update(existing);
            return NoContent();
        }

        /// <summary>
        /// Delete a credential
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var credential = _credentialRepository.GetById(id);
            if (credential == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Credential with ID {id} not found"));
            }

            _credentialRepository.Delete(id);
            return NoContent();
        }
    }
}
