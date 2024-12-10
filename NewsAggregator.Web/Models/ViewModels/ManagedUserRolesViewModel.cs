using NewsAggregator.Web.Models.Domain;

namespace NewsAggregator.Web.Models.ViewModels
{
    public class ManageUserRolesViewModel
    {
        public ApplicationUser User { get; set; }
        public List<string> Roles { get; set; }
    }

}
