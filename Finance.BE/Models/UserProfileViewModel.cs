using WebModels;

namespace WEB.Models
{
    public class UserProfileViewModel: UserProfile
    {
        public string CustomerActiveString { set; get; }
        public string CustomerTypeString { set; get; }
    }
}