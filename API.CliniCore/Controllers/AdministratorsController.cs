using System;
using System.Collections.Generic;
using System.Linq;
using API.CliniCore.Common;
using Core.CliniCore.Api;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.DTOs.Administrators;
using Core.CliniCore.Mapping;
using Core.CliniCore.Requests.Administrators;
using Core.CliniCore.Service;
using Microsoft.AspNetCore.Mvc;

namespace API.CliniCore.Controllers
{
    [ApiController]
    [Route(ApiRoutes.Administrators.BasePath)]
    public class AdministratorsController : ControllerBase
    {
        private readonly ProfileService _profileService;

        public AdministratorsController(ProfileService profileService)
        {
            _profileService = profileService;
        }

        /// <summary>
        /// Get all administrators
        /// </summary>
        [HttpGet]
        public ActionResult<IEnumerable<AdministratorDto>> GetAll()
        {
            var admins = _profileService.GetAllAdministrators();
            var dtos = admins.Select(a => a.ToDto());
            return Ok(dtos);
        }

        /// <summary>
        /// Get an administrator by ID
        /// </summary>
        [HttpGet(ApiRoutes.Administrators.ById)]
        public ActionResult<AdministratorDto> GetById(Guid id)
        {
            var admin = _profileService.GetProfileById(id) as AdministratorProfile;
            if (admin == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Administrator with ID {id} not found"));
            }

            return Ok(admin.ToDto());
        }

        /// <summary>
        /// Create a new administrator
        /// </summary>
        [HttpPost]
        public ActionResult<AdministratorDto> Create([FromBody] CreateAdministratorRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiErrorResponse.FromErrors("Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            // Create the administrator profile (use client-provided ID if available for credential linking)
            var admin = new AdministratorProfile
            {
                Id = request.Id ?? Guid.NewGuid(),
                Username = request.Username,
                Department = request.Department
            };

            // Set profile values (common entries)
            admin.SetValue(CommonEntryType.Name.GetKey(), request.Name);

            if (!string.IsNullOrEmpty(request.Address))
                admin.SetValue(CommonEntryType.Address.GetKey(), request.Address);

            if (request.BirthDate != default)
                admin.SetValue(CommonEntryType.BirthDate.GetKey(), request.BirthDate);

            // Set administrator-specific entry
            if (!string.IsNullOrEmpty(request.Email))
                admin.SetValue(AdministratorEntryType.Email.GetKey(), request.Email);

            // Set permissions
            foreach (var permStr in request.Permissions)
            {
                if (Enum.TryParse<Permission>(permStr, true, out var perm))
                    admin.GrantedPermissions.Add(perm);
            }

            // Add profile (credential creation is handled separately via /api/auth/register)
            _profileService.AddProfile(admin);

            return CreatedAtAction(nameof(GetById), new { id = admin.Id }, admin.ToDto());
        }

        /// <summary>
        /// Update an existing administrator
        /// </summary>
        [HttpPut("{id}")]
        public ActionResult<AdministratorDto> Update(Guid id, [FromBody] UpdateAdministratorRequest request)
        {
            var admin = _profileService.GetProfileById(id) as AdministratorProfile;
            if (admin == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Administrator with ID {id} not found"));
            }

            // Apply updates
            admin.ApplyUpdate(request);

            // Persist the changes
            _profileService.UpdateProfile(admin);

            return Ok(admin.ToDto());
        }

        /// <summary>
        /// Delete an administrator
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var error = _profileService.RemoveProfile(id);

            if (error == "Profile not found")
                return NotFound(ApiErrorResponse.FromMessage($"Administrator with ID {id} not found"));

            if (error != null)
                return BadRequest(ApiErrorResponse.FromMessage($"Cannot delete administrator: {error}"));

            return NoContent();
        }

        /// <summary>
        /// Get administrators by department
        /// </summary>
        [HttpGet(ApiRoutes.Administrators.ByDepartment)]
        public ActionResult<IEnumerable<AdministratorDto>> GetByDepartment(string department)
        {
            var admins = _profileService.GetAllAdministrators()
                .Where(a => a.Department.Equals(department, StringComparison.OrdinalIgnoreCase));
            var dtos = admins.Select(a => a.ToDto());
            return Ok(dtos);
        }

        /// <summary>
        /// Get administrators by permission
        /// </summary>
        [HttpGet(ApiRoutes.Administrators.ByPermission)]
        public ActionResult<IEnumerable<AdministratorDto>> GetByPermission(string permission)
        {
            if (!Enum.TryParse<Permission>(permission, true, out var perm))
            {
                return BadRequest(ApiErrorResponse.FromMessage($"Invalid permission: {permission}"));
            }

            var admins = _profileService.GetAllAdministrators()
                .Where(a => a.GrantedPermissions.Contains(perm));
            var dtos = admins.Select(a => a.ToDto());
            return Ok(dtos);
        }
    }
}
