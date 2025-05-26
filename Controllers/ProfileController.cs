using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TestApi.Data;
using TestApi.Models;
using TestApi.DTOS;

namespace TestApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly TestDbContext _dbContext;
        private readonly UserManager<AppUser> _userManager;

        public ProfileController(TestDbContext dbContext, UserManager<AppUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        // Create or Update Profile
        [HttpPost]
        public async Task<IActionResult> CreateOrUpdateProfile([FromBody] ProfleDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized("User not found");

            var profile = await _dbContext.Profiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (profile == null)
            {
                profile = new UserProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Bio = dto.Bio,
                    ProfilePictureUrl = dto.ProfileImageUrl,
                    Website = dto.Website,
                    Location = dto.Location,
                    TwitterHandle = dto.TwitterHandle,
                    LinkedInHandle = dto.LinkedInHandle,
                    CreatedAt = (DateTime)dto.CreatedAt,
                    DisplayName = dto.DisplayName,

                };
                await _dbContext.Profiles.AddAsync(profile);
            }
            else
            {
                profile.Bio = dto.Bio;
                profile.ProfilePictureUrl = dto.ProfileImageUrl;
                profile.Website = dto.Website;
                profile.Location = dto.Location;
                profile.TwitterHandle = dto.TwitterHandle;
                profile.LinkedInHandle = dto. LinkedInHandle;
                profile.CreatedAt = (DateTime)dto.CreatedAt;
                profile.DisplayName = dto.DisplayName;
                _dbContext.Profiles.Update(profile);
            }

            await _dbContext.SaveChangesAsync();
            // Return a clean DTO instead of the entity
            var resultDto = new ProfleDto
            {
                DisplayName = profile.DisplayName,
                Bio = profile.Bio,
                ProfileImageUrl = profile.ProfilePictureUrl,
                Website = profile.Website,
                Location = profile.Location,
                TwitterHandle = profile.TwitterHandle,
                LinkedInHandle = profile.LinkedInHandle,
                CreatedAt = profile.CreatedAt
            };

            return Ok(resultDto);
        }

        // Get Profile
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var profile = await _dbContext.Profiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (profile == null)
                return NotFound("Profile not found.");
            var resultDto = new ProfleDto
            {
                DisplayName = profile.DisplayName,
                Bio = profile.Bio,
                ProfileImageUrl = profile.ProfilePictureUrl,
                Website = profile.Website,
                Location = profile.Location,
                TwitterHandle = profile.TwitterHandle,
                LinkedInHandle = profile.LinkedInHandle,
                CreatedAt = profile.CreatedAt
            };

            return Ok(resultDto);
        
        }

        // Delete Profile and User
        [HttpDelete]
        public async Task<IActionResult> DeleteProfileAndUser()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var userId = Guid.Parse(user.Id); // assuming AppUser.Id is a string of Guid format

            // Delete related posts
            var posts = _dbContext.Posts.Where(p => p.UserId == userId);
            _dbContext.Posts.RemoveRange(posts);

            // Delete related comments
            var comments = _dbContext.Comments.Where(c => c.UserId == userId);
            _dbContext.Comments.RemoveRange(comments);

            // Delete profile
            var profile = await _dbContext.Profiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (profile != null)
                _dbContext.Profiles.Remove(profile);

            await _dbContext.SaveChangesAsync();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest("Failed to delete user.");

            return Ok("User and all related data deleted.");
        }
    }
}
