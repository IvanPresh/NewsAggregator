using NewsAggregator.Web.Models.Domain;
using NewsAggregator.Web.Models.ViewModels;

namespace NewsAggregator.Web.Models.ViewModels
{
    public class SavedArticles
    {
        public Guid SavedArticleId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }
        public string Url { get; set; }
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }
    }

}
