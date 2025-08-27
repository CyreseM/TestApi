using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using TestApi.Data;
using TestApi.DTOS;
using TestApi.Models;

// PostController exposes CRUD endpoints for blog posts, with server-managed identity and timestamps.
// Next steps to get closer to Medium-like functionality:
// 1) Add pagination and filtering (e.g., query params: page, pageSize, tag, author, sort)
// 2) Add tags and reading time fields to Post model and include in DTOs
// 3) Add slug generation for SEO-friendly URLs and fetch by slug
// 4) Add soft-delete and UpdatedAt; track edit history if needed
// 5) Add bookmarking/reading lists and claps (reaction model per-user)
// 6) Add full-text search (e.g., EF.Functions.Contains or external search)
// 7) Add rate limiting and caching for popular endpoints
namespace TestApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly TestDbContext _dbContext;

        public PostController(UserManager<AppUser> userManager, TestDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        private static string GenerateSlug(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return Guid.NewGuid().ToString("n");
            var slug = new string(title.ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray());
            slug = System.Text.RegularExpressions.Regex.Replace(slug, "-+", "-").Trim('-');
            return string.IsNullOrWhiteSpace(slug) ? Guid.NewGuid().ToString("n") : slug;
        }

        [HttpGet]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string q = null, [FromQuery] string tag = null)
        {
            // Pagination + simple filtering by query and tag
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 100);

            var query = _dbContext.Posts.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(p => p.Title.Contains(q) || p.Description.Contains(q) || p.Content.Contains(q));
            }
            if (!string.IsNullOrWhiteSpace(tag))
            {
                query = query.Where(p => p.TagsCsv != null && ("," + p.TagsCsv + ",").Contains("," + tag + ","));
            }

            var total = await query.CountAsync();
            var posts = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(p => p.Comments).ThenInclude(c => c.Replies)
                .ToListAsync();

            if (posts == null)
            {
                return NotFound("No posts found.");
            }
           var postDtos = posts.Select(post => new CreatePostDto
    {
        Title = post.Title,
        Slug = post.Slug,
        Description = post.Description,
        ImageUrl = post.ImageUrl,
        Content = post.Content,
        Tags = string.IsNullOrEmpty(post.TagsCsv) ? new List<string>() : post.TagsCsv.Split(',').Select(t => t.Trim()).Where(t => t.Length > 0).ToList(),
        CreatedAt = post.CreatedAt,
        UserName = post.UserName,
        Likes = post.Likes,
        DisLikes = post.Dislikes,
        UserId = post.UserId,
        Comments = post.Comments.Where(c => c.ParentCommentId == null).Select(MapComment).ToList()
    }).ToList();
    return Ok(new { total, page, pageSize, items = postDtos });
        }


        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CreatePostDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPostById([FromRoute] Guid id)
        {
            var posts = await _dbContext.Posts.Include(p => p.Comments).ThenInclude(c => c.Replies).FirstOrDefaultAsync(p => p.Id == id);
            if (posts == null)
            {
                return NotFound("No posts found.");
            }
            var postDto = new CreatePostDto
            {
                Title = posts.Title,
                Slug = posts.Slug,
                Description = posts.Description,
                ImageUrl = posts.ImageUrl,
                Content = posts.Content,
                Tags = string.IsNullOrEmpty(posts.TagsCsv) ? new List<string>() : posts.TagsCsv.Split(',').Select(t => t.Trim()).Where(t => t.Length > 0).ToList(),
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

        [HttpGet("slug/{slug}")]
        [ProducesResponseType(typeof(CreatePostDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPostBySlug([FromRoute] string slug)
        {
            var post = await _dbContext.Posts
                .Include(p => p.Comments).ThenInclude(c => c.Replies)
                .FirstOrDefaultAsync(p => p.Slug == slug);
            if (post == null)
                return NotFound("No posts found.");

            var postDto = new CreatePostDto
            {
                Title = post.Title,
                Slug = post.Slug,
                Description = post.Description,
                ImageUrl = post.ImageUrl,
                Content = post.Content,
                Tags = string.IsNullOrEmpty(post.TagsCsv) ? new List<string>() : post.TagsCsv.Split(',').Select(t => t.Trim()).Where(t => t.Length > 0).ToList(),
                CreatedAt = post.CreatedAt,
                UserName = post.UserName,
                Likes = post.Likes,
                DisLikes = post.Dislikes,
                UserId= post.UserId,
                Comments = post.Comments
                                .Where(c => c.ParentCommentId == null)
                                .Select(MapComment)
                                .ToList()
            };
            return Ok(postDto);
        }

        [HttpGet("PostByUserId/{userId}")]
        public async Task<IActionResult> GetPostsByUserId([FromRoute] string userId)
        {
            // Filter posts by author (userId). Consider adding pagination and sort order
            var posts = await _dbContext.Posts
                .Where(p => p.UserId == userId)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Replies)
                .ToListAsync();

            if (!posts.Any())
                return NotFound("No posts found.");

            var postDtos = posts.Select(post => new CreatePostDto
            {
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
        [ProducesResponseType(typeof(Post), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto createPostDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }
            // Never trust client-supplied Id/CreatedAt; set them server-side to ensure integrity
            var post = new Post
            {
                Id = Guid.NewGuid(),
                Title = createPostDto.Title,
                Slug = GenerateSlug(createPostDto.Title),
                Description = createPostDto.Description,
                ImageUrl = createPostDto.ImageUrl,
                Content = createPostDto.Content,
                CreatedAt = DateTime.UtcNow,
                TagsCsv = createPostDto.Tags != null ? string.Join(",", createPostDto.Tags.Select(t => t.Trim()).Where(t => t.Length > 0)) : null,
                UserId = user.Id,
                UserName = user.UserName
            };

            await _dbContext.Posts.AddAsync(post);
            await _dbContext.SaveChangesAsync();

            // Return Location header to the new resource (use id or future slug)
            return CreatedAtAction(nameof(GetPostById), new { id = post.Id }, post);
        }
        [Authorize]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePost([FromRoute] Guid id, [FromBody] UpdatePostDto updateDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized("User not found.");

            var post = await _dbContext.Posts.FindAsync(id);
            if (post == null)
                return NotFound("Post not found.");

            // Optional: Check if current user is the owner of the post
            if (post.UserId != user.Id)
                return Forbid("You are not allowed to update this post.");

            // Update the post fields. Avoid changing server-managed fields
            post.Title = updateDto.Title;
            post.Slug = GenerateSlug(updateDto.Title);
            post.Description = updateDto.Description;
            post.ImageUrl = updateDto.ImageUrl;
            post.Content = updateDto.Content;
            // Do not modify CreatedAt on update; keep original creation time

            // Optionally update other editable fields like UpdatedAt, if you track that
            // post.UpdatedAt = DateTime.UtcNow;

            _dbContext.Posts.Update(post);
            await _dbContext.SaveChangesAsync();

            return NoContent(); // 204 - successful but no response body
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost([FromRoute] Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            var post = await _dbContext.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if (post == null)
            {
                return NotFound("Post not found.");
            }

            if (post.UserId != user.Id)
            {
                return Forbid("You are not allowed to delete this post.");
            }

            _dbContext.Posts.Remove(post);
            await _dbContext.SaveChangesAsync();

            return NoContent(); 
        }


        [Authorize]
         [HttpPatch("{id}/reactions")]
        public async Task<IActionResult> UpdateCommentReactions(Guid id, [FromBody] UpdateReactionsDto dto)
                {
                    // This endpoint currently sets aggregate counts directly.
                    // To prevent gaming, migrate to per-user Reaction entity and compute totals.
                    var post = await _dbContext.Posts.FindAsync(id);

                    if (post == null)
                    {
                        return NotFound("Post not found.");
                    }

                    if (dto.Likes.HasValue)
                    {
                        post.Likes = dto.Likes.Value;
                    }

                    if (dto.Dislikes.HasValue)
                    {
                        post.Dislikes = dto.Dislikes.Value;
                    }

                    await _dbContext.SaveChangesAsync();

                    return Ok(new
                    {
                        post.Id,
                        post.Likes,
                        post.Dislikes
                    });
                }
        
        [Authorize]
        [HttpPost("{id}/react")]
        public async Task<IActionResult> ReactToPost([FromRoute] Guid id, [FromQuery] ReactionState state)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var post = await _dbContext.Posts.FindAsync(id);
            if (post == null) return NotFound();

            var existing = await _dbContext.PostReactions.FirstOrDefaultAsync(r => r.PostId == id && r.UserId == user.Id);
            if (existing == null)
            {
                existing = new PostReaction { Id = Guid.NewGuid(), PostId = id, UserId = user.Id, State = state };
                await _dbContext.PostReactions.AddAsync(existing);
            }
            else
            {
                existing.State = state;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            // Recompute aggregate counts
            post.Likes = await _dbContext.PostReactions.CountAsync(r => r.PostId == id && r.State == ReactionState.Like);
            post.Dislikes = await _dbContext.PostReactions.CountAsync(r => r.PostId == id && r.State == ReactionState.Dislike);
            await _dbContext.SaveChangesAsync();
            return Ok(new { post.Id, post.Likes, post.Dislikes, yourReaction = state });
        }

        [Authorize]
        [HttpPost("{id}/bookmark")]
        public async Task<IActionResult> ToggleBookmark([FromRoute] Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var post = await _dbContext.Posts.FindAsync(id);
            if (post == null) return NotFound();

            var existing = await _dbContext.Bookmarks.FirstOrDefaultAsync(b => b.PostId == id && b.UserId == user.Id);
            if (existing == null)
            {
                await _dbContext.Bookmarks.AddAsync(new Bookmark { Id = Guid.NewGuid(), PostId = id, UserId = user.Id });
                await _dbContext.SaveChangesAsync();
                return Ok(new { bookmarked = true });
            }
            else
            {
                _dbContext.Bookmarks.Remove(existing);
                await _dbContext.SaveChangesAsync();
                return Ok(new { bookmarked = false });
            }
        }

        [Authorize]
        [HttpGet("bookmarks")]
        public async Task<IActionResult> GetBookmarks([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 100);

            var baseQuery = _dbContext.Bookmarks.Where(b => b.UserId == user.Id);
            var total = await baseQuery.CountAsync();
            var bookmarkPostIds = await baseQuery
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => b.PostId)
                .ToListAsync();

            var posts = await _dbContext.Posts
                .Where(p => bookmarkPostIds.Contains(p.Id))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var items = posts.Select(p => new CreatePostDto
            {
                Title = p.Title,
                Slug = p.Slug,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                Content = p.Content,
                Tags = string.IsNullOrEmpty(p.TagsCsv) ? new List<string>() : p.TagsCsv.Split(',').Select(t => t.Trim()).Where(t => t.Length > 0).ToList(),
                CreatedAt = p.CreatedAt,
                UserName = p.UserName,
                Likes = p.Likes,
                DisLikes = p.Dislikes,
                UserId = p.UserId,
                Comments = new List<CommentDto>()
            }).ToList();

            return Ok(new { total, page, pageSize, items });
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