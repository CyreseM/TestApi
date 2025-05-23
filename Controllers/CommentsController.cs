using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            if (parent.Replies != null)
                return BadRequest("This comment already has a reply.");

            // Create and save the reply comment
            var reply = new Comment
            {
                PostId = parent.PostId,       // associate with same post
                ParentCommentId = parent.Id,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.Comments.Add(reply);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetComment), "Comments",
                                     new { id = reply.Id }, reply);
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
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.Comments.Add(comment);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetComment), new { id = comment.Id },  new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt
            });
        }
         
        [HttpGet("comments/{id}")]
        public async Task<ActionResult<Comment>> GetComment(int id)
        {
            var comment = await _dbContext.Comments.FindAsync(id);
            if (comment == null) return NotFound();
            return comment;
        }
    }
}
