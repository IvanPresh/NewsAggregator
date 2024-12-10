using Article = NewsAggregator.Web.Models.Domain.Article;

namespace NewsAggregator.Web.Services
{
    public class ArticleService : IArticleService
    {
        private readonly NewsService _newsService;

        public ArticleService(NewsService newsService)
        {
            _newsService = newsService;
        }

        // Ensure the return type is Task<Article?>
        public async Task<Article?> GetArticleDetailsByUrlAsync(string url)
        {
            // Fetch articles from the API
            var newsResponse = await _newsService.GetTopHeadlinesAsync();

            if (newsResponse == null || newsResponse.Articles == null || !newsResponse.Articles.Any())
            {
                return null;
            }

            // Find the article by URL
            var articleDto = newsResponse.Articles.FirstOrDefault(a => a.Url == url);

            if (articleDto == null)
            {
                return null; // Return null if the article is not found
            }

            // Map DTO to domain model
            var article = new Article
            {
                Title = articleDto.Title,
                Description = articleDto.Description,
                Url = articleDto.Url,
                Source = articleDto.Source.Name,
                Content = articleDto.Content,
                PublishedAt = (DateTime)articleDto.PublishedAt
            };

            return article; // Return the article details
        }
    }
}
