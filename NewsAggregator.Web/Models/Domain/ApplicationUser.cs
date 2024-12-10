using Microsoft.AspNetCore.Identity;

namespace NewsAggregator.Web.Models.Domain
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? SubscriptionType { get; set; } // Optional (Nullable)

        // Navigation properties
        public ICollection<SearchHistory>? SearchHistories { get; set; } // One-to-many relationship with SearchHistory
        public ICollection<SavedArticle>? SavedArticles { get; set; } // One-to-many relationship with SavedArticle
        public ICollection<Article>? Articles { get; set; } // One-to-many relationship with SavedArticle
    }


}
