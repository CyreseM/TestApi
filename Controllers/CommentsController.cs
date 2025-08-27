using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using TestApi.Data;
using TestApi.DTOS;
using TestApi.Models;

// CommentsController manages top-level comments and one-level replies.
// Next steps:
// 1) Add per-user reactions (like/dislike) to avoid trusting client-provided counts
// 2) Add pagination for comments on large posts
// 3) Add moderation (reporting, hiding) and profanity filtering
// 4) Consider allowing deeper nesting with a path or hierarchyid-like approach
namespace TestApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly TestDbContext _dbContext;

        public CommentsController(TestDbContext dbContext)
        {
       
            _dbContext = dbContext;
        }

        [Authorize]
        [HttpPost("{commentId}/reply")]
        public async Task<IActionResult> ReplyToComment(Guid commentId, [FromBody] CreateReplyDto dto)
        {
            // Ensure the parent comment exists
            var parent = await _dbContext.Comments
                                 .Include(c => c.Replies)
                                 .FirstOrDefaultAsync(c => c.Id == commentId);
            if (parent == null)
                return NotFound($"Comment {commentId} not found.");
            // Enforce only one level of reply
            if (parent.ParentCommentId != null)
                return BadRequest("Cannot reply to a reply.");
            //if (parent.Replies != null)
            //    return BadRequest("This comment already has a reply.");

            // Create and save the reply comment
            var reply = new Comment
            {
                PostId = parent.PostId,       // associate with same post
                ParentCommentId = parent.Id,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow,
                UserId = dto.UserId
            };
            _dbContext.Comments.Add(reply);
            await _dbContext.SaveChangesAsync();
            var replyDto = new CommentReplyDto
            {
                Id = reply.Id,
                Content = reply.Content,
                CreatedAt = reply.CreatedAt,
                ParentCommentId = reply.ParentCommentId,
                PostId = reply.PostId,
                UserId = reply.UserId
            };
            return CreatedAtAction(nameof(GetComment), "Comments",
                                     new { id = reply.Id }, replyDto);
        }

        [Authorize]
        [HttpPost("{postId}/comments")]
        public async Task<IActionResult> CreateComment(Guid postId, [FromBody] CreateCommentDto dto)
        {
            // Ensure the post exists
            var post = await _dbContext.Posts.FindAsync(postId);
            if (post == null)
                return NotFound($"Post {postId} not found.");

            // Create and save the new comment
            var comment = new Comment
            {
                PostId = postId,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow,
                UserId = dto.UserId

            };
            _dbContext.Comments.Add(comment);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetComment), new { id = comment.Id },  new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                 UserId   = comment.UserId
            });
        }
         
        [HttpGet("comments/{id}")]
        public async Task<ActionResult<Comment>> GetComment([FromRoute] Guid id)
        {
            var comment = await _dbContext.Comments.FindAsync(id);
            if (comment == null) return NotFound();
            return comment;
        }
        [Authorize]
        [HttpPut("{commentId}")]
        public async Task<IActionResult> UpdateComment(Guid commentId, [FromBody] UpdateCommentDto dto)
        {
            var comment = await _dbContext.Comments.FindAsync(commentId);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (comment == null) return NotFound();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            if (comment.UserId != userId)
                return Forbid(); // user not owner

            comment.Content = dto.Content;
            // update other allowed fields

            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

    
        [Authorize]
        [HttpDelete("comments/{id}")]
        public async Task<ActionResult<Comment>> DeleteComment([FromRoute] Guid id)
        {
            var comment = await _dbContext.Comments
        .Include(c => c.Replies)
        .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null) return NotFound();

            // Delete replies first
            if (comment.Replies != null && comment.Replies.Any())
            {
                _dbContext.Comments.RemoveRange(comment.Replies);
            }

            _dbContext.Comments.Remove(comment);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        [Authorize]
        [HttpPatch("comments/{id}/reactions")]
        public async Task<IActionResult> UpdateCommentReactions(Guid id, UpdateReactionsDto dto)
        {
            var comment = await _dbContext.Comments.FindAsync(id);

            if (comment == null)
            {
                return NotFound("Comment not found.");
            }

            // Per-user reaction handling
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Interpret likes/dislikes to a ReactionState
            ReactionState state = ReactionState.None;
            if (dto.Likes.HasValue && dto.Likes.Value > 0) state = ReactionState.Like;
            if (dto.Dislikes.HasValue && dto.Dislikes.Value > 0) state = ReactionState.Dislike;

            var existing = await _dbContext.CommentReactions.FirstOrDefaultAsync(r => r.CommentId == id && r.UserId == userId);
            if (existing == null)
            {
                existing = new CommentReaction { Id = Guid.NewGuid(), CommentId = id, UserId = userId, State = state };
                await _dbContext.CommentReactions.AddAsync(existing);
            }
            else
            {
                existing.State = state;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            // Recompute aggregate counts
            comment.Likes = await _dbContext.CommentReactions.CountAsync(r => r.CommentId == id && r.State == ReactionState.Like);
            comment.Dislikes = await _dbContext.CommentReactions.CountAsync(r => r.CommentId == id && r.State == ReactionState.Dislike);

            await _dbContext.SaveChangesAsync();

            return Ok(new { comment.Id, comment.Likes, comment.Dislikes, yourReaction = state });
        }

    }
}
