namespace JackHenry2.Models
{
    public class Statistics
    {
        public List<Post> MostPopularPosts { get; set; } = new List<Post>();
        public int PostUpvotes { get; set; } = 0;
        public List<string> MostPopularAuthors { get; set; } = new List<string>();
        public int AuthorPosts { get; set; } = 0;
    }
}