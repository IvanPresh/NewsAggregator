using ASPNetCoreMVCSample.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsAggregator.Web.Services;
using ASPNetCoreMVCSample.DTO;
using NewsAggregator.Web.Models.Domain;
using X.PagedList;
using NewsAggregator.Web.Models;
using Article = NewsAggregator.Web.Models.DTOs.Article;
using NewsAggregator.Web.Models.ViewModels;

public class NewsController : Controller
{
    private readonly NewsService _newsService;
    private readonly EmailService _emailService;
    private readonly NewsAggregateDbContext _context;
    private readonly IArticleService _articleService;

    public NewsController(NewsAggregateDbContext context, IArticleService articleService, NewsService newsService, EmailService emailService)
    {
        _context = context;
        _newsService = newsService;
        _emailService = emailService;
        _articleService = articleService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string searchTerm, string category, int page = 1)
    {
        var newsResponse = await _newsService.GetTopHeadlinesAsync(category, searchTerm);
        var newsArticles = newsResponse?.Articles ?? new List<Article>();

        var pagedArticles = newsArticles.ToPagedList(page, 10);

        var pagedArticlesViewModel = new PagedArticlesViewModel
        {
            Articles = pagedArticles.ToList(),
            CurrentPage = page,
            TotalPages = pagedArticles.PageCount
        };

        ViewBag.SearchTerm = searchTerm;
        ViewBag.Category = category;
        return View(pagedArticlesViewModel);
    }

    public async Task<IActionResult> ReadArticle(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return RedirectToAction(nameof(Index));
        }

        var articleDto = await _articleService.GetArticleDetailsByUrlAsync(url);

        if (articleDto == null)
        {
            return NotFound();
        }

        var detailedArticleViewModel = new DetailedArticleViewModel
        {
            Title = articleDto.Title,
            Description = articleDto.Description,
            Content = articleDto.Content,
            Source = articleDto.Source,
            Url = articleDto.Url,
            PublishedAt = articleDto.PublishedAt
        };

        return View(detailedArticleViewModel);
    }

    public async Task<IActionResult> Search(string query, int? page)
    {
        if (string.IsNullOrEmpty(query)) return RedirectToAction("Index");

        var newsResponse = await _newsService.GetTopHeadlinesAsync("", query);
        var pageNumber = page ?? 1;
        var pageSize = 10;

        if (newsResponse != null)
        {
            var searchHistory = new SearchHistory
            {
                SearchQuery = query,
                SearchDate = DateTime.Now
            };

            _context.SearchHistories.Add(searchHistory);
            await _context.SaveChangesAsync();

            var searchPaged = newsResponse.Articles.ToPagedList(pageNumber, pageSize);

            var model = new PagedArticlesViewModel
            {
                Articles = searchPaged.ToList(),
                CurrentPage = pageNumber,
                TotalPages = searchPaged.PageCount,
                SearchTerm = query,
                Category = ""
            };

            return View(model);
        }

        return View("Error");
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Create(Article article)
    {
        if (ModelState.IsValid)
        {
            try
            {
                _context.Add(article);
                await _context.SaveChangesAsync();

                var emailDto = new EmailDto
                {
                    From = "your-email@example.com",
                    To = "recipient-email@example.com",
                    Body = $"New article created: {article.Title}"
                };

                _emailService.SendEmail(emailDto);
                return RedirectToAction(nameof(SavedArticles));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while saving the article. " + ex.Message);
            }
        }
        return View(article);
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var article = await _context.SavedArticles.FindAsync(id);
        if (article == null) return NotFound();

        return View(article);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Edit(Guid id, Article article)
    {
        if (id != article.ArticleId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(article);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(SavedArticles));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArticleExists(article.ArticleId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        return View(article);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var article = await _context.SavedArticles.FindAsync(id);
        if (article == null) return NotFound();

        _context.SavedArticles.Remove(article);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(SavedArticles));
    }

    private bool ArticleExists(Guid id)
    {
        return _context.SavedArticles.Any(e => e.SavedArticleId == id);
    }
}
