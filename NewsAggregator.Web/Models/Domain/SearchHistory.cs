namespace NewsAggregator.Web.Models.Domain
{
    public class SearchHistory
    {
        public Guid SearchHistoryId { get; set; }
        public Guid UserId { get; set; } // Foreign Key referencing ApplicationUser (GUID)
        public string SearchQuery { get; set; }
        public DateTime? SearchDate { get; set; }

        // Defining Relationship property
        public ApplicationUser User { get; set; } // Many-to-one relationship with ApplicationUser
    }

}
