using System.Threading.Tasks;
using NewsAggregator.Web.Models.Domain;

namespace NewsAggregator.Web.Services
{
    public interface IArticleService
    {
        // Return type should be Task<Article?>
        Task<Article?> GetArticleDetailsByUrlAsync(string url);
    }
}
