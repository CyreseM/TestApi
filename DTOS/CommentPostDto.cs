namespace TestApi.DTOS
{
    public class CommentPostDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CommentDto> Comments { get; set; }
    }
}
