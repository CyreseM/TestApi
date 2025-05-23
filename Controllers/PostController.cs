using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using TestApi.Data;
using TestApi.DTOS;
using TestApi.Models;

namespace TestApi.Controllers
{
    public class PostController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly TestDbContext _dbContext;

        public PostController(UserManager<AppUser> userManager, TestDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

       
        [HttpGet]
        [Route("posts")]
        public async Task<IActionResult> GetAllPosts()
        {

            var posts = await _dbContext.Posts.Include(p => p.Comments).ThenInclude(c => c.Replies).ToListAsync();

            if (posts == null)
            {
                return NotFound("No posts found.");
            }
           var postDtos = posts.Select(post => new CreatePostDto
    {
        Id = post.Id,
        Title = post.Title,
        Description = post.Description,
        ImageUrl = post.ImageUrl,
        Content = post.Content,
        CreatedAt = post.CreatedAt,
        UserName = post.UserName,
        Likes = post.Likes,
        DisLikes = post.Dislikes,
        UserId = post.UserId,
        Comments = post.Comments.Where(c => c.ParentCommentId == null).Select(MapComment).ToList()
    }).ToList();

    return Ok(postDtos);
        }


        [HttpGet]
        [Route("posts/{id}")]
        public async Task<IActionResult> GetPostById([FromRoute] Guid id)
        {
            var posts = await _dbContext.Posts.Include(p => p.Comments).ThenInclude(c => c.Replies).FirstOrDefaultAsync(p => p.Id == id);
            if (posts == null)
            {
                return NotFound("No posts found.");
            }
            var postDto = new CreatePostDto
            {
                Id = posts.Id,
                Title = posts.Title,
                Description = posts.Description,
                ImageUrl = posts.ImageUrl,
                Content = posts.Content,
                CreatedAt = posts.CreatedAt,
                UserName = posts.UserName,
                Likes = posts.Likes,
                DisLikes = posts.Dislikes,
                UserId= posts.UserId,
                Comments = posts.Comments
                                .Where(c => c.ParentCommentId == null)
                                .Select(MapComment)
                                .ToList()
            };

            return Ok(postDto);
        }

        [HttpGet]
        [Route("posts/PostByUserId/{userId}")]
        public async Task<IActionResult> GetPostsByUserId([FromRoute] Guid userId)
        {
            var posts = await _dbContext.Posts
                .Where(p => p.UserId == userId)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Replies)
                .ToListAsync();

            if (!posts.Any())
                return NotFound("No posts found.");

            var postDtos = posts.Select(post => new CreatePostDto
            {
                Id = post.Id,
                Title = post.Title,
                Description = post.Description,
                ImageUrl = post.ImageUrl,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                UserName = post.UserName,
                Likes = post.Likes,
                DisLikes = post.Dislikes,
                UserId = post.UserId,
                Comments = post.Comments
                                 .Where(c => c.ParentCommentId == null)
                                 .Select(MapComment)
                                 .ToList()
            }).ToList();

            return Ok(postDtos);
        }

        [Authorize]
        [HttpPost]
        [Route("posts")]

        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto createPostDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }
            var post = new Post
            {
                Id = createPostDto.Id,
                Title = createPostDto.Title,
                Description = createPostDto.Description,
                ImageUrl = createPostDto.ImageUrl,
                Content = createPostDto.Content,
                CreatedAt = createPostDto.CreatedAt,
                UserId = Guid.Parse(user.Id),
                UserName = user.UserName
            };

            await _dbContext.Posts.AddAsync(post);
            await _dbContext.SaveChangesAsync();

            // Optionally, return the created post or its location
            return CreatedAtAction(nameof(GetPostById), new { id = post.Id }, post);
        }
        [Authorize]
        [HttpPut("posts/{id}")]
        public async Task<IActionResult> UpdatePost([FromRoute] Guid id, [FromBody] UpdatePostDto updateDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized("User not found.");

            var post = await _dbContext.Posts.FindAsync(id);
            if (post == null)
                return NotFound("Post not found.");

            // Optional: Check if current user is the owner of the post
            if (post.UserId != Guid.Parse(user.Id))
                return Forbid("You are not allowed to update this post.");

            // Update the post fields
            post.Title = updateDto.Title;
            post.Description = updateDto.Description;
            post.ImageUrl = updateDto.ImageUrl;
            post.Content = updateDto.Content;
            post.CreatedAt = updateDto.CreatedAt;

            // Optionally update other editable fields like UpdatedAt, if you track that
            // post.UpdatedAt = DateTime.UtcNow;

            _dbContext.Posts.Update(post);
            await _dbContext.SaveChangesAsync();

            return NoContent(); // 204 - successful but no response body
        }

        [Authorize]
        [HttpDelete]
        [Route("posts/{id}")]
        public async Task<IActionResult> DeletePost([FromRoute] Guid id)
        {
            var post = await _dbContext.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if (post == null)
            {
                return NotFound("Post not found.");
            }

            _dbContext.Posts.Remove(post);
            await _dbContext.SaveChangesAsync();

            return NoContent(); 
        }



        private CommentDto MapComment(Comment comment)
        {
            return new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                Likes = comment.Likes,
                DisLikes = comment.Dislikes,
                UserId = comment.UserId,
                Replies = comment.Replies?.Select(MapComment).ToList() ?? new List<CommentDto>()
            };
        }


    }
}