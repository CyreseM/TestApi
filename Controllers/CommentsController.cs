using System.Security.Claims;
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
    public class CommentsController : Controller
    {
        private readonly TestDbContext _dbContext;

        public CommentsController(TestDbContext dbContext)
        {
       
            _dbContext = dbContext;
        }


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
        [HttpPut("{commentId}")]
        public async Task<IActionResult> UpdateComment(Guid commentId, [FromBody] UpdateCommentDto dto)
        {
            var comment = await _dbContext.Comments.FindAsync(commentId);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (comment == null) return NotFound();

            if (comment.UserId != Guid.Parse( userId))
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

            if (dto.Likes.HasValue)
            {
                comment.Likes = dto.Likes.Value;
            }

            if (dto.Dislikes.HasValue)
            {
                comment.Dislikes = dto.Dislikes.Value;
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                comment.Id,
                comment.Likes,
                comment.Dislikes
            });
        }

    }
}
