public interface IPostService
{
    public Task<string> CreateNewPost(Author author);
    public Task<Post> GetPostAsync(string id);
    public Task<List<Post>> GetPostsAsync(int pageNumber = 1, int pageSize = 10);
    public Task<List<Post>> GetFeed(Author author, int pageNumber = 1, int pageSize = 10);
    public Task<int> GetFeedCount(Author author);
    public Task<int> GetPostsCount();

}
