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

            var posts = await _dbContext.Posts.Include(p => p.Comments).ToListAsync();

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
        Comments = post.Comments.Select(c => new CommentDto
        {
            Id = c.Id,
            Content = c.Content,
            CreatedAt = c.CreatedAt,
            Likes = c.Likes,
            DisLikes = c.Dislikes
        }).ToList()
    }).ToList();

    return Ok(postDtos);
        }


        [HttpGet]
        [Route("posts/{id}")]
        public async Task<IActionResult> GetPostById([FromRoute] Guid id)
        {
            var posts = await _dbContext.Posts.Include(p => p.Comments).FirstOrDefaultAsync(p => p.Id == id);
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
                Comments = posts.Comments.Select(c => new CommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    Likes = c.Likes,
                    DisLikes = c.Dislikes,
                   
                }).ToList()
            };

            return Ok(postDto);
        }

        [HttpGet]
        [Route("posts/PostByUserId/{userId}")]
        public async Task<IActionResult> GetPostByUserId([FromRoute] Guid userId)
        {
            var posts = await _dbContext.Posts.FirstOrDefaultAsync(p => p.UserId == userId);
            if (posts == null)
            {
                return NotFound("No posts found.");
            }
            return Ok(posts);
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





    }
}