﻿using NewsAggregator.Web.Models.DTOs;

namespace NewsAggregator.Web.Models
{
    public class PagedArticlesViewModel
    {
        public List<Article> Articles { get; set; } // The list of articles to display
        public int CurrentPage { get; set; } // Current page number
        public int TotalPages { get; set; } // Total number of pages
        public string SearchTerm { get; set; } // Search term for filtering
        public string Category { get; set; } // Selected category for filtering
    }

}
