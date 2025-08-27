namespace TestApi.DTOS
{
    public class UpdatePostDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string Content { get; set; }
        // CreatedAt is server-managed and not updatable
    }
}
